using ModSink.Core.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Collections.ObjectModel;

namespace ModSink.Common.Client
{
    public class DownloadManager : IDownloadManager
    {
        private readonly IDownloader downloader;
        private byte simultaneousDownloads = 1;

        public DownloadManager(IDownloader downloader)
        {
            this.downloader = downloader;
            this.Downloads.CollectionChanged += (_, __) => CheckDownloadsToStart();
        }

        public event EventHandler<IDownload> DownloadStarted;

        public ObservableCollection<IDownload> Downloads { get; } = new ObservableCollection<IDownload>();

        public void CheckDownloadsToStart()
        {
            var toStart = this.simultaneousDownloads - this.Downloads.Count(d => d.State == DownloadState.Downloading);
            for (int i = 0; i < toStart; i++)
            {
                var d = NextDownload();
                if (d == null) break;
                d.Start(this.downloader);
                d.Progress.Subscribe(_ => { }, _ => CheckDownloadsToStart(), () => CheckDownloadsToStart());
                OnDownloadStarted(d);
            }
        }

        private IDownload NextDownload()
        {
            return this.Downloads.Where(d => d.State == DownloadState.Queued).FirstOrDefault();
        }

        private void OnDownloadStarted(IDownload e) => DownloadStarted?.Invoke(this, e);
    }
}