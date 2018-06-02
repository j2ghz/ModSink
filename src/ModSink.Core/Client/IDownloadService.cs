using DynamicData;

namespace ModSink.Core.Client
{
    public interface IDownloadService
    {
        IObservableList<IDownload> Downloads { get; }

        void Add(IDownload download);
    }
}