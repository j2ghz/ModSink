using System;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;

namespace ModSink.Common.Client
{
    public class DownloadService : ReactiveObject, IDisposable
    {
        private readonly CompositeDisposable disposable = new CompositeDisposable();
        private int simultaneousDownloads;

        public DownloadService(IDownloader downloader, IObservable<IChangeSet<QueuedDownload>> downloadQueue,
            LocalFilesManager localFilesManager)
        {
            SimultaneousDownloads = 5;
            QueuedDownloads = downloadQueue.AsObservableList().DisposeWith(disposable);
            QueuedDownloads.Connect().Subscribe().DisposeWith(disposable);
            ActiveDownloads = downloadQueue
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Top(SimultaneousDownloads)
                .Transform(qd => new ActiveDownload(qd, localFilesManager.GetTemporaryFileStream(qd.FileSignature),
                    () => localFilesManager.AddNewFile(qd.FileSignature), downloader))
                .DisposeMany()
                .AsObservableList()
                .DisposeWith(disposable);
            ActiveDownloads.Connect().Subscribe().DisposeWith(disposable);
        }

        public IObservableList<QueuedDownload> QueuedDownloads { get; }

        public IObservableList<ActiveDownload> ActiveDownloads { get; }

        public int SimultaneousDownloads
        {
            get => simultaneousDownloads;
            set
            {
                this.RaiseAndSetIfChanged(ref simultaneousDownloads, value);
                if (ServicePointManager.DefaultConnectionLimit < value)
                    ServicePointManager.DefaultConnectionLimit = value;
            }
        }

        public void Dispose()
        {
            disposable.Dispose();
        }
    }
}