using System;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Anotar.Serilog;
using DynamicData;
using Humanizer;
using ModSink.Common.Models;
using ModSink.Common.Models.Client;
using ModSink.Common.Models.Group;
using ModSink.Common.Models.Repo;
using ReactiveUI;

namespace ModSink.Common.Client
{
    public class ClientService : ReactiveObject, IDisposable
    {
        private readonly CompositeDisposable disposable = new CompositeDisposable();
        private readonly IDownloader downloader;
        private readonly LocalFilesManager localFilesManager;
        private readonly IFormatter serializationFormatter;

        public ClientService(IDownloader downloader, IFormatter serializationFormatter, DirectoryInfo localStorage)
        {
            this.downloader = downloader;
            this.serializationFormatter = serializationFormatter;
            LogTo.Warning("Creating pipeline");
            Repos = GroupUrls.Connect()
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Transform(g => new Uri(g))
                .TransformAsync(Load<Group>)
                .TransformMany(g => g.RepoInfos.Select(r => new Uri(g.BaseUri, r.Uri)))
                .TransformAsync(Load<Repo>)
                .AsObservableList()
                .DisposeWith(disposable);
            Repos.Connect().Subscribe().DisposeWith(disposable);
            var onlineFiles = Repos.Connect()
                .ObserveOn(RxApp.TaskpoolScheduler)
                .TransformMany(r => r.Files)
                .Transform(kvp => new OnlineFile(kvp.Key, kvp.Value))
                .AsObservableList()
                .DisposeWith(disposable);
            onlineFiles.Connect().Subscribe().DisposeWith(disposable);
            var filesRequired = Repos.Connect()
                .ObserveOn(RxApp.TaskpoolScheduler)
                .TransformMany(r => r.Modpacks)
                .AutoRefresh(m => m.Selected)
                .Filter(m => m.Selected)
                .TransformMany(m => m.Mods)
                .TransformMany(m => m.Mod.Files.Values)
                .AsObservableList()
                .DisposeWith(disposable);
            localFilesManager =
                new LocalFilesManager(new FileAccessService(localStorage), filesRequired, onlineFiles, downloader)
                    .DisposeWith(disposable);
        }

        public IObservableList<Repo> Repos { get; }


        public ISourceList<string> GroupUrls { get; } = new SourceList<string>();
        public DownloadService DownloadService { get; }

        public void Dispose()
        {
            disposable.Dispose();
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