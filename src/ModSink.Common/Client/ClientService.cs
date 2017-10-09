using ModSink.Core.Client;
using System;
using System.Collections.Generic;
using System.Text;
using ModSink.Core.Models.Repo;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using ReactiveUI;
using DynamicData;
using System.Data.HashFunction;

namespace ModSink.Common.Client
{
    public class ClientService : ReactiveObject, IClientService
    {
        private readonly SourceList<Repo> repos = new SourceList<Repo>();

        public ClientService(IDownloadService downloadService, ILocalStorageService localStorageService, IDownloader downloader, IFormatter serializationFormatter)
        {
            this.DownloadService = downloadService;
            this.LocalStorageService = localStorageService;
            this.Downloader = downloader;
            this.SerializationFormatter = serializationFormatter;
        }

        public IDownloader Downloader { get; }
        public IDownloadService DownloadService { get; }
        public ILocalStorageService LocalStorageService { get; }
        public IObservableList<Modpack> Modpacks => this.Repos.Connect().TransformMany(r => r.Modpacks).AsObservableList();
        public IObservableList<Repo> Repos => this.repos.AsObservableList();
        public IFormatter SerializationFormatter { get; }

        public async Task DownloadMissingFiles(Modpack modpack)
        {
            foreach (var mod in modpack.Mods)
            {
                foreach (var fh in mod.Mod.Files)
                {
                    var hash = fh.Value;
                    if (await this.LocalStorageService.IsFileAvailable(hash)) continue;
                    this.DownloadService.Add(new Download(
                        GetDownloadUri(hash),
                        new Lazy<Task<Stream>>(async () => await this.LocalStorageService.Write(hash)),
                        hash.ToString()));
                }
            }
        }

        public Uri GetDownloadUri(HashValue hash)
        {
            foreach (var repo in this.Repos.Items)
            {
                if (repo.Files.TryGetValue(hash, out Uri relativeUri))
                {
                    return new Uri(repo.BaseUri, relativeUri);
                }
            }
            throw new KeyNotFoundException($"Key {hash} was not found in a Files dictionary of any Repo");
        }

        public IObservable<DownloadProgress> LoadRepo(Uri uri)
        {
            var obs = Observable.Create<DownloadProgress>(async o =>
            {
                var tempFile = Path.GetTempFileName();
                var stream = new FileStream(tempFile, FileMode.Create);
                var progress = this.Downloader.Download(uri, stream, "Repo");
                progress.Subscribe(o.OnNext, o.OnError, () => { });
                await progress;
                stream = new FileStream(tempFile, FileMode.Open, FileAccess.Read);
                var repo = (Repo)this.SerializationFormatter.Deserialize(stream);
                repo.BaseUri = new Uri(uri, ".");
                this.repos.Add(repo);
                o.OnCompleted();
            }).Publish();
            obs.Connect();
            return obs;
        }
    }
}