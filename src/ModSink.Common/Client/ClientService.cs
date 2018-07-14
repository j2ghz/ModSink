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
using Humanizer;
using ModSink.Common.Models;
using ModSink.Common.Models.Client;
using ModSink.Common.Models.Group;
using ModSink.Common.Models.Repo;

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
                .Transform(g => new Uri(g))
                .TransformAsync(Load<Group>)
                .TransformMany(g => g.RepoInfos.Select(r => new Uri(g.BaseUri, r.Uri)))
                .AddKey(u => u)
                .TransformAsync(Load<Repo>)
                .AsObservableCache()
                .DisposeWithThrowExceptions(disposable);
            OnlineFiles = Repos.Connect()
                .TransformMany(r => r.Files, f => f.Key)
                .Transform(kvp => new OnlineFile(kvp.Key, kvp.Value))
                .AsObservableCache()
                .DisposeWithThrowExceptions(disposable);
            Modpacks = Repos.Connect()
                .RemoveKey()
                .TransformMany(r => r.Modpacks)
                .AsObservableList()
                .DisposeWithThrowExceptions(disposable);
            QueuedDownloads = Modpacks.Connect()
                .AutoRefresh(m => m.Selected)
                .Filter(m => m.Selected)
                .TransformMany(m => m.Mods)
                .TransformMany(m => m.Mod.Files.Values)
                .AddKey(fs => fs)
                .Filter(fs => !filesAvailable.Items.Contains(fs))
                .InnerJoin(OnlineFiles.Connect(), of => of.FileSignature,
                    (fs, of) => new QueuedDownload(fs, of.Uri))
                .AsObservableCache()
                .DisposeWithThrowExceptions(disposable);
            ActiveDownloads = QueuedDownloads.Connect()
                .Top(Comparer<QueuedDownload>.Default, 2)
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
        public ISourceList<string> GroupUrls { get; } = new SourceList<string>();

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