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
    public class ClientManager : IClientManager
    {
        public ClientManager(IDownloadManager downloadManager, ILocalRepoManager localRepoManager, IDownloader downloader, IFormatter serializationFormatter)
        {
            this.DownloadManager = downloadManager;
            this.LocalRepoManager = localRepoManager;
            this.Downloader = downloader;
            this.SerializationFormatter = serializationFormatter;
        }

        public IDownloader Downloader { get; }
        public IDownloadManager DownloadManager { get; }
        public ILocalRepoManager LocalRepoManager { get; }
        public ICollection<Modpack> Modpacks => this.Repos?.SelectMany(r => r.Modpacks).ToList();
        public ICollection<Repo> Repos { get; } = new List<Repo>();
        public IFormatter SerializationFormatter { get; }

        public void DownloadMissingFiles(Modpack modpack)
        {
            var files = modpack.Mods.SelectMany(mod => mod.Mod.Files.Select(f => f.Value));
            var filesToDownload = files.Where(f => !this.LocalRepoManager.IsFileAvailable(f));
            var downloads = filesToDownload.Select(f => new Download(GetDownloadUri(f), new Lazy<Stream>(() => this.LocalRepoManager.Write(f))));
            foreach (var download in downloads)
            {
                Console.WriteLine(download.Source);
                this.DownloadManager.Downloads.Add(download);
            }
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
                var stream = new MemoryStream();
                var progress = this.Downloader.Download(uri, stream);
                progress.Subscribe(o.OnNext, o.OnError, () => { });
                await progress;
                stream.Position = 0;
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