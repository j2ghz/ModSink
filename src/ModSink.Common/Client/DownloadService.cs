using System;
using System.IO;
using System.Net;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;

namespace ModSink.Common.Client
{
    public class DownloadService : ReactiveObject
    {
        private readonly IDownloader downloader;
        private int simultaneousDownloads;

        public DownloadService(IDownloader downloader, IObservable<IChangeSet<QueuedDownload>> downloadQueue,
            DirectoryInfo tempDownloadsDirectory)
        {
            this.downloader = downloader;
            SimultaneousDownloads = 5;
            QueuedDownloads = downloadQueue.AsObservableList();
            QueuedDownloads.Connect().Subscribe();
            ActiveDownloads = downloadQueue
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Top(SimultaneousDownloads)
                .Transform(qd => new ActiveDownload(qd, tempDownloadsDirectory, downloader))
                .DisposeMany()
                .AsObservableList();
            ActiveDownloads.Connect().Subscribe();
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
    }
}