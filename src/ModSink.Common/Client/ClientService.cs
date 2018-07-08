using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Anotar.Serilog;
using DynamicData;
using Humanizer;
using ModSink.Common.Models;
using ModSink.Common.Models.Group;
using ModSink.Common.Models.Repo;
using ReactiveUI;

namespace ModSink.Common.Client
{
    public class ClientService : ReactiveObject
    {
        private readonly IDownloader downloader;
        private readonly IFormatter serializationFormatter;
        private LocalFilesManager localFilesManager;

        public ClientService(IDownloader downloader, IFormatter serializationFormatter,
            DirectoryInfo localFilesDirectory, DirectoryInfo tempDownloadsDirectory)
        {
            this.downloader = downloader;
            this.serializationFormatter = serializationFormatter;
            LogTo.Warning("Creating pipeline");
            Repos = GroupUrls
                .Connect()
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Transform(g => new Uri(g))
                .TransformAsync(Load<Group>)
                .TransformMany(g => g.RepoInfos.Select(r => new Uri(g.BaseUri, r.Uri)))
                .TransformAsync(Load<Repo>)
                .AsObservableList();
            Repos.Connect().Subscribe();
            var allFiles = Repos.Connect().TransformMany(r => r.Files).AsObservableList();
            allFiles.Connect().Subscribe();
            var downloadQueue = Repos.Connect()
                .TransformMany(r => r.Modpacks)
                .AutoRefresh(m => m.Selected)
                .Filter(m => m.Selected)
                .TransformMany(m => m.Mods)
                .TransformMany(m => m.Mod.Files.Values)
                .Transform(fs => allFiles.Items.First(kvp => kvp.Key.Equals(fs)))
                .Transform(kvp => new QueuedDownload(kvp.Key, kvp.Value))
                .AsObservableList();
            downloadQueue.Connect().Subscribe();
            DownloadService = new DownloadService(downloader, downloadQueue.Connect(), tempDownloadsDirectory);


            //localFilesManager = new LocalFilesManager(requiredFiles);
        }

        public IObservableList<Repo> Repos { get; }


        public ISourceList<string> GroupUrls { get; } = new SourceList<string>();
        public DownloadService DownloadService { get; }

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