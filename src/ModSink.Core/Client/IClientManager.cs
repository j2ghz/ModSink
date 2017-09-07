using ModSink.Core.Client;
using ModSink.Core.Models.Repo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace ModSink.Core.Client
{
    public interface IClientManager
    {
        IDownloadManager DownloadManager { get; }
        ILocalRepoManager LocalRepoManager { get; }
        ICollection<Modpack> Modpacks { get; }
        ICollection<Repo> Repos { get; }

        void DownloadMissingFiles(Modpack modpack);

        Uri GetDownloadUri(HashValue hash);

        Task<Repo> LoadRepo(Uri uri, IObserver<DownloadProgress> progress);
    }
}