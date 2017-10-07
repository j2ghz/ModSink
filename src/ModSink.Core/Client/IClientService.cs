using DynamicData;
using DynamicData.Binding;
using ModSink.Core.Client;
using ModSink.Core.Models.Repo;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace ModSink.Core.Client
{
    public interface IClientService : IReactiveObject
    {
        IDownloadService DownloadService { get; }
        ILocalStorageService LocalStorageService { get; }

        IObservableList<Modpack> Modpacks { get; }
        IObservableList<Repo> Repos { get; }

        void DownloadMissingFiles(Modpack modpack);

        Uri GetDownloadUri(HashValue hash);

        IObservable<DownloadProgress> LoadRepo(Uri uri);
    }
}