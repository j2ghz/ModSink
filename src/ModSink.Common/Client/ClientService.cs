using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using DynamicData;
using ModSink.Core.Client;
using ModSink.Core.Models.Repo;
using ReactiveUI;
using Serilog;

namespace ModSink.Common.Client
{
    public class ClientService : ReactiveObject, IClientService
    {
        private readonly SourceList<Repo> repos = new SourceList<Repo>();

        public ClientService(IDownloadService downloadService, ILocalStorageService localStorageService,
            IDownloader downloader, IFormatter serializationFormatter)
        {
            DownloadService = downloadService;
            LocalStorageService = localStorageService;
            Downloader = downloader;
            SerializationFormatter = serializationFormatter;
        }

        public IDownloader Downloader { get; }
        public IFormatter SerializationFormatter { get; }
        private ILogger log => Log.ForContext<ClientService>();
        public IDownloadService DownloadService { get; }
        public ILocalStorageService LocalStorageService { get; }
        public IObservableList<Modpack> Modpacks => Repos.Connect().TransformMany(r => r.Modpacks).AsObservableList();
        public IObservableList<Repo> Repos => repos.AsObservableList();

        public async Task DownloadMissingFiles(Modpack modpack)
        {
            log.Information("Gathering files to download for {modpack}", modpack.Name);
            foreach (var mod in modpack.Mods)
            foreach (var fh in mod.Mod.Files)
            {
                var fileSignature = fh.Value;
                var res = await LocalStorageService.WriteIfMissingOrInvalid(fileSignature);
                if (!res.available)
                    DownloadService.Add(new Download(GetDownloadUri(fileSignature), res.stream,
                        fileSignature.ToString()));
            }
        }

        public Uri GetDownloadUri(FileSignature fileSignature)
        {
            foreach (var repo in Repos.Items)
                if (repo.Files.TryGetValue(fileSignature, out var relativeUri))
                    return new Uri(repo.BaseUri, relativeUri);
            throw new KeyNotFoundException($"Key {fileSignature} was not found in a Files dictionary of any Repo");
        }

        public IConnectableObservable<DownloadProgress> LoadRepo(Uri uri)
        {
            log.Information("Loading repo from {url}", uri);
            return Observable.Create<DownloadProgress>(async o =>
            {
                var dispose = new CompositeDisposable();
                var tempFile = Path.GetTempFileName();
                log.Debug("Downloading repo to temp file {path}", tempFile);
                var stream = new FileStream(tempFile, FileMode.Create);
                var progress = Downloader.Download(uri, stream);
                progress.Subscribe(o.OnNext, o.OnError).DisposeWith(dispose);
                await progress;
                stream = new FileStream(tempFile, FileMode.Open, FileAccess.Read);
                log.Debug("Deserializing downloaded repo");
                var repo = (Repo) SerializationFormatter.Deserialize(stream);
                repo.BaseUri = new Uri(uri, ".");
                repos.Add(repo);
                o.OnCompleted();
            }).Publish();
        }
    }
}