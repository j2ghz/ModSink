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
        IObservableList<Repo> Repos { get; }
        ISourceList<string> GroupUrls { get; }
        Task DownloadMissingFiles(Modpack modpack);
        Uri GetDownloadUri(FileSignature fileSignature);
    }
}