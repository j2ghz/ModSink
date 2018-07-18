using System;
using System.Collections;
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
            Repos = GroupUrls.Connect()
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Transform(g => new Uri(g))
                .TransformAsync(Load<Group>)
                .TransformMany(g => g.RepoInfos.Select(r => new Uri(g.BaseUri, r.Uri)), repoUri => repoUri)
                .TransformAsync(Load<Repo>)
                .OnItemUpdated((repo, _) => LogTo.Information("Repo from {url} has been loaded", repo.BaseUri))
                .AsObservableCache()
                .DisposeWithThrowExceptions(disposable);
            OnlineFiles = Repos.Connect()
                .ObserveOn(RxApp.TaskpoolScheduler)
                .TransformMany(
                    repo => repo.Files.Select(kvp => new OnlineFile(kvp.Key, new Uri(repo.BaseUri, kvp.Value))),
                    of => of.FileSignature)
                .AsObservableCache()
                .DisposeWithThrowExceptions(disposable);
            Modpacks = Repos.Connect()
                .ObserveOn(RxApp.TaskpoolScheduler)
                .RemoveKey()
                .TransformMany(r => r.Modpacks)
                .AsObservableList()
                .DisposeWithThrowExceptions(disposable);
            QueuedDownloads = Modpacks.Connect()
                .ObserveOn(RxApp.TaskpoolScheduler)
                .AutoRefresh(m => m.Selected)
                .Filter(m => m.Selected)
                .TransformMany(m => m.Mods)
                .TransformMany(m => m.Mod.Files.Values)
                .AddKey(fs => fs)
                .LeftJoin(filesAvailable.Connect(), f => f,
                    (required, available) => !available.HasValue
                        ? Optional<FileSignature>.Create(required)
                        : Optional<FileSignature>.None)
                .Filter(opt => opt.HasValue)
                .Transform(opt => opt.Value)
                .InnerJoin(OnlineFiles.Connect(), of => of.FileSignature,
                    (fs, of) => new QueuedDownload(fs, of.Uri))
                .OnItemUpdated((qd, _) =>
                    LogTo.Information("Download added to queue ({file} from {url})", qd.FileSignature, qd.Source))
                .AsObservableCache()
                .DisposeWithThrowExceptions(disposable);
            ActiveDownloads = QueuedDownloads.Connect()
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Sort(Comparer<QueuedDownload>.Create((a,b)=>0),SortOptimisations.ComparesImmutableValuesOnly)
                .Top(5)
                .Transform(qd => new ActiveDownload(qd, GetTemporaryFileStream(qd.FileSignature),
                    () => AddNewFile(qd.FileSignature), downloader))
                .DisposeMany()
                .AsObservableCache()
                .DisposeWithThrowExceptions(disposable);
        }

        public IObservableList<Modpack> Modpacks { get; }
        public IObservableCache<OnlineFile, FileSignature> OnlineFiles { get; }
        public IObservableCache<QueuedDownload, FileSignature> QueuedDownloads { get; }
        public IObservableCache<Repo, Uri> Repos { get; }
        public IObservableCache<ActiveDownload, FileSignature> ActiveDownloads { get; }
        public ISourceCache<string, string> GroupUrls { get; } = new SourceCache<string, string>(u => u);

        public void Dispose()
        {
            disposable.Dispose();
        }

        private void AddNewFile(FileSignature fileSignature)
        {
            fileAccessService.TemporaryFinished(fileSignature);
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
                await downloader.Download(uri, mem);
                LogTo.Debug("Deserializing, size: {size}", mem.Length.Bytes().Humanize("G03"));
                mem.Position = 0;
                var t = (T) serializationFormatter.Deserialize(mem);
                t.BaseUri = new Uri(uri, ".");
                return t;
            }
        }
    }
}