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
using ReactiveUI;

namespace ModSink.Common.Client
{
    public class ClientService : IDisposable
    {
        private readonly CompositeDisposable disposable = new CompositeDisposable();
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
            Repos = DynamicDataChain.GetReposFromGroups(GroupUrls.Connect(), Load<Group>, Load<Repo>).AsObservableCache().DisposeWith(disposable);
            OnlineFiles = DynamicDataChain.GetOnlineFileFromRepos(Repos.Connect())
                .AsObservableCache()
                .DisposeWith(disposable);
            Modpacks = DynamicDataChain.GetModpacksFromRepos(Repos.Connect()).AsObservableCache()
                .DisposeWith(disposable);
            QueuedDownloads = DynamicDataChain.GetDownloadsFromModpacks(Modpacks.Connect())
                .AsObservableCache()
                .DisposeWith(disposable)
                .Connect()
                .LeftJoin(filesAvailable.Connect(), f => f,
                    (required, available) =>
                    {
                        LogTo.Verbose(
                            available.HasValue ? "File {signature} is available" : "File {signature} is not available",
                            required);
                        return !available.HasValue
                            ? Optional<FileSignature>.Create(required)
                            : Optional<FileSignature>.None;
                    })
                .Filter(opt => opt.HasValue)
                .Transform(opt => opt.Value)
                .InnerJoin(OnlineFiles.Connect(), of => of.FileSignature,
                    (fs, of) => new QueuedDownload(fs, of.Uri))
                .AsObservableCache()
                .DisposeWith(disposable);
            ActiveDownloads = QueuedDownloads.Connect()
                .Sort(Comparer<QueuedDownload>.Create((_, __) => 0))
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
                .AsObservableCache()
                .DisposeWith(disposable);
        }

        public IObservableCache<ActiveDownload, FileSignature> ActiveDownloads { get; }
        public ISourceCache<string, string> GroupUrls { get; } = new SourceCache<string, string>(u => u);
        public IObservableCache<Modpack, Guid> Modpacks { get; }
        public IObservableCache<OnlineFile, FileSignature> OnlineFiles { get; }
        public IObservableCache<QueuedDownload, FileSignature> QueuedDownloads { get; }
        public IObservableCache<Repo, Uri> Repos { get; }

        public void Dispose()
        {
            disposable.Dispose();
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

        private async Task<T> Load<T>(Uri uri) where T : IBaseUri
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