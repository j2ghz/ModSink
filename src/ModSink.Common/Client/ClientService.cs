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

            var groups = GroupUrls
                .Connect()
                .Transform(g => new Uri(g))
                .TransformAsync(Load<Group>);
            var repos = groups
                .TransformMany(g => g.RepoInfos.Select(r => new Uri(g.BaseUri, r.Uri)))
                .TransformAsync(Load<Repo>);
            var allFiles = repos
                .TransformMany(r => r.Files)
                .AsObservableList();
            var modpacks = repos
                .TransformMany(r => r.Modpacks);
            var selectedModpacks = modpacks
                .Filter(m => m.Selected);
            var requiredFiles = selectedModpacks
                .TransformMany(m => m.Mods)
                .TransformMany(m => m.Mod.Files.Values);
            var downloads = requiredFiles
                .Transform(fs => allFiles.Items.Single(kvp => kvp.Key.Equals(fs)))
                .Transform(kvp => new QueuedDownload(kvp.Key, kvp.Value));
            DownloadService = new DownloadService(downloader, downloads, tempDownloadsDirectory);


            //localFilesManager = new LocalFilesManager(requiredFiles);
        }


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