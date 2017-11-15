using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
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
            foreach (var mod in modpack.Mods)
            foreach (var fh in mod.Mod.Files)
            {
                var fileSignature = fh.Value;

                try
                {
                    if (await LocalStorageService.IsFileAvailable(fileSignature)) continue;
                }
                catch (FileSignatureException e)
                {
                    log.Warning(e, "File found, but with a wrong signature, deleting");
                    await LocalStorageService.Delete(fileSignature);
                }
                DownloadService.Add(new Download(
                    GetDownloadUri(fileSignature),
                    new Lazy<Task<Stream>>(async () => await LocalStorageService.Write(fileSignature)),
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

        public IObservable<DownloadProgress> LoadRepo(Uri uri)
        {
            var obs = Observable.Create<DownloadProgress>(async o =>
            {
                var tempFile = Path.GetTempFileName();
                var stream = new FileStream(tempFile, FileMode.Create);
                var progress = Downloader.Download(uri, stream);
                progress.Subscribe(o.OnNext, o.OnError, () => { });
                await progress;
                stream = new FileStream(tempFile, FileMode.Open, FileAccess.Read);
                var repo = (Repo) SerializationFormatter.Deserialize(stream);
                repo.BaseUri = new Uri(uri, ".");
                repos.Add(repo);
                o.OnCompleted();
            }).Publish();
            obs.Connect();
            return obs;
        }
    }
}