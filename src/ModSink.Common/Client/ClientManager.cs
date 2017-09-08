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
            var downloads = filesToDownload.Select(f => new Download(GetDownloadUri(f), new Lazy<System.IO.Stream>(() => this.LocalRepoManager.Create(f))));
            downloads.ForEach(this.DownloadManager.Downloads.Add);
        }

        public Uri GetDownloadUri(HashValue hash)
        {
            foreach (var repo in this.Repos)
            {
                if (repo.Files.TryGetValue(hash, out Uri uri))
                {
                    return uri;
                }
            }
            throw new KeyNotFoundException($"Key {hash} was not found in a Files dictionary of any Repo");
        }

        public async Task<Repo> LoadRepo(Uri uri, IObserver<DownloadProgress> progress)
        {
            var stream = new MemoryStream();
            var obs = this.Downloader.Download(uri, stream);
            obs.Subscribe(progress);
            await obs;
            return this.SerializationFormatter.Deserialize(stream) as Repo;
        }
    }
}