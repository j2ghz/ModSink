using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Anotar.Serilog;
using DynamicData;
using DynamicData.Kernel;
using Humanizer;
using ModSink.Common.Models;
using ModSink.Common.Models.Client;
using ModSink.Common.Models.Group;
using ModSink.Common.Models.Repo;

namespace ModSink.Common.Client
{
    public class ClientService : IDisposable
    {
        private readonly CompositeDisposable d = new CompositeDisposable();
        private readonly IDownloader downloader;
        private readonly IFileAccessService fileAccessService;

        private readonly SourceCache<FileSignature, HashValue> filesAvailable =
            new SourceCache<FileSignature, HashValue>(fs => fs.Hash);

        private readonly IFormatter serializationFormatter;

        public ClientService(IDownloader downloader, IFormatter serializationFormatter, DirectoryInfo localStorage)
        {
            fileAccessService = new FileAccessService(localStorage);
            this.downloader = downloader;
            this.serializationFormatter = serializationFormatter;
            filesAvailable.Edit(l => { l.AddOrUpdate(fileAccessService.FilesAvailable()); });
            LogTo.Warning("Creating pipeline");
            Repos = GroupUrls.Connect()
                .Transform(g => new Uri(g))
                .TransformAsync(Load<Group>)
                .TransformMany(g => g.RepoInfos.Select(r => g.CombineBaseUri(r.Uri)), repoUri => repoUri)
                .TransformAsync(Load<Repo>)
                .OnItemUpdated((repo, _) => LogTo.Information("Repo from {url} has been loaded", repo.BaseUri))
                .AsObservableCache();
            d.Add(Repos);
            OnlineFiles = Repos.Connect()
                .TransformMany(
                    repo => repo.Files.Select(kvp => new OnlineFile(kvp.Key, repo.CombineBaseUri(kvp.Value))),
                    of => of.FileSignature).AsObservableCache();
            d.Add(OnlineFiles);
            Modpacks = DynamicDataChain.GetModpacksFromRepos(Repos.Connect()).AsObservableCache();
            d.Add(Modpacks);
            QueuedDownloads = DynamicDataChain.GetDownloadsFromModpacks(Modpacks.Connect())
                .LeftJoin(filesAvailable.Connect(), f => f,
                    (required, available) => !available.HasValue
                        ? Optional<FileSignature>.Create(required)
                        : Optional<FileSignature>.None)
                .Filter(opt => opt.HasValue)
                .Transform(opt => opt.Value)
                .InnerJoin(OnlineFiles.Connect(), of => of.FileSignature,
                    (fs, of) => new QueuedDownload(fs, of.Uri))
                .AsObservableCache();
            d.Add(QueuedDownloads);
            ActiveDownloads = QueuedDownloads.Connect()
                .Sort(Comparer<QueuedDownload>.Create(
                    (a, b) => a.FileSignature.Length.CompareTo(b.FileSignature.Length)))
                .Top(5)
                .LogVerbose("activeDownloadsSimple")
                .Transform(qd =>
                {
                    var destination = new Lazy<Stream>(() => GetTemporaryFileStream(qd.FileSignature));
                    return new ActiveDownload(
                        downloader.Download(qd.Source, destination,
                            qd.FileSignature.Length),
                        () =>
                        {
                            destination.Value.Dispose();
                            LogTo.Verbose("ActiveDownload {name} finished", qd.FileSignature.Hash);
                            AddNewFile(qd.FileSignature);
                        }, qd.FileSignature.ToString());
                })
                .LogVerbose("activeDownloads")
                .AsObservableCache();
            d.Add(ActiveDownloads);
        }

        public IObservableCache<ActiveDownload, FileSignature> ActiveDownloads { get; }
        public ISourceCache<string, string> GroupUrls { get; } = new SourceCache<string, string>(u => u);
        public IObservableCache<Modpack, Guid> Modpacks { get; }
        public IObservableCache<OnlineFile, FileSignature> OnlineFiles { get; }
        public IObservableCache<QueuedDownload, FileSignature> QueuedDownloads { get; }
        public IObservableCache<Repo, Uri> Repos { get; }

        public void Dispose()
        {
            d.Dispose();
        }

        private void AddNewFile(FileSignature fileSignature)
        {
            fileAccessService.TemporaryFinished(fileSignature);
            LogTo.Verbose("File {name} is now available", fileSignature.Hash);
            filesAvailable.AddOrUpdate(fileSignature);
        }

        private Stream GetTemporaryFileStream(FileSignature argFileSignature)
        {
            return fileAccessService.Write(argFileSignature, true);
        }

        private async Task<T> Load<T>(Uri uri) where T : WithBaseUri
        {
            LogTo.Information("Loading {T} from {url}", typeof(T), uri);
            using (var mem = new MemoryStream())
            {
                await downloader.Download(uri, new Lazy<Stream>(() => mem));
                LogTo.Debug("Deserializing, size: {size}", mem.Length.Bytes().Humanize("G03"));
                mem.Position = 0;
                var t = (T) serializationFormatter.Deserialize(mem);
                t.BaseUri = new Uri(uri, ".");
                return t;
            }
        }
    }
}