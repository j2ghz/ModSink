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

namespace ModSink.Common.Client
{
    public class ClientService : IClientService
    {
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
        public ICollection<Modpack> Modpacks => this.Repos?.SelectMany(r => r.Modpacks).ToList();
        public ICollection<Repo> Repos { get; } = new List<Repo>();
        public IFormatter SerializationFormatter { get; }

        public void DownloadMissingFiles(Modpack modpack)
        {
            modpack.Mods
                .SelectMany(mod => mod.Mod.Files.Select(f => f.Value))
                .Where(f => !this.LocalStorageService.IsFileAvailable(f))
                .Select(f => new Download(GetDownloadUri(f), new Lazy<Stream>(() => this.LocalStorageService.Write(f)), f.ToString()))
                .ForEach(this.DownloadService.Add);
        }

        public Uri GetDownloadUri(HashValue hash)
        {
            foreach (var repo in this.Repos)
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
                this.Repos.Add(repo);
                o.OnCompleted();
            }).Publish();
            obs.Connect();
            return obs;
        }
    }
}