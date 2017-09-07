using ModSink.Core.Client;
using System;
using System.Collections.Generic;
using System.Text;
using ModSink.Core.Models.Repo;
using System.Threading.Tasks;
using System.Linq;

namespace ModSink.Common.Client
{
    public class ClientManager : IClientManager
    {
        public ClientManager(IDownloadManager downloadManager, ILocalRepoManager localRepoManager)
        {
            this.DownloadManager = downloadManager;
            this.LocalRepoManager = localRepoManager;
        }

        public IDownloadManager DownloadManager { get; }
        public ILocalRepoManager LocalRepoManager { get; }
        public ICollection<Modpack> Modpacks => this.Repos?.SelectMany(r => r.Modpacks).ToList();

        public ICollection<Repo> Repos { get; } = new List<Repo>();

        public void DownloadMissingFiles(Modpack modpack)
        {
            var files = modpack.Mods.SelectMany(mod => mod.Mod.Files.Select(f => f.Value));
            var filesToDownload = files.Where(f => !this.LocalRepoManager.IsFileAvailable(f));
            var downloads = filesToDownload.Select(f => new Download(GetDownloadUri(f), new Lazy<System.IO.Stream>(() => this.LocalRepoManager.Create(f))));
            downloads.ForEach(this.DownloadManager.Downloads.Add);
        }

        public Uri GetDownloadUri(HashValue hash)
        {
            foreach (var repo in Repos)
            {
                if (repo.Files.TryGetValue(hash, out Uri uri))
                {
                    return uri;
                }
            }
            throw new KeyNotFoundException($"Key {hash} was not found in a Files dictionary of any Repo");
        }

        public Task<Repo> LoadRepo(Uri uri, IObserver<DownloadProgress> progress)
        {
            throw new NotImplementedException();
        }
    }
}