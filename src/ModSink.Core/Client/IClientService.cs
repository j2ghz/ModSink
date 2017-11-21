using System;
using System.Threading.Tasks;
using DynamicData;
using ModSink.Core.Models.Repo;
using ReactiveUI;

namespace ModSink.Core.Client
{
    public interface IClientService : IReactiveObject
    {
        IDownloadService DownloadService { get; }
        ILocalStorageService LocalStorageService { get; }
        IObservableList<Modpack> Modpacks { get; }
        IObservableList<Repo> Repos { get; }
        ISourceList<string> RepoUrls { get; }
        Task DownloadMissingFiles(Modpack modpack);
        Uri GetDownloadUri(FileSignature fileSignature);
    }
}