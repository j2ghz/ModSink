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
    public interface IClientService
    {
        IDownloadService DownloadService { get; }
        ILocalStorageService LocalStorageService { get; }
        ICollection<Modpack> Modpacks { get; }
        ICollection<Repo> Repos { get; }

        void DownloadMissingFiles(Modpack modpack);

        Uri GetDownloadUri(HashValue hash);

        IObservable<DownloadProgress> LoadRepo(Uri uri);
    }
}