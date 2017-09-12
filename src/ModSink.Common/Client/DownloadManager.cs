using ModSink.Core.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;

namespace ModSink.Common.Client
{
    public class DownloadManager : IDownloadManager
    {
        private readonly IDownloader downloader;
        private byte simultaneousDownloads = 1;

        public DownloadManager(IDownloader downloader)
        {
            this.downloader = downloader;
        }

        public event EventHandler<IDownload> DownloadStarted;

        public IList<IDownload> Downloads => new List<IDownload>();

        public void CheckDownloadsToStart()
        {
            var toStart = this.Downloads.Count(d => d.State == DownloadState.Downloading) - this.simultaneousDownloads;
            for (int i = 0; i < toStart; i++)
            {
                var d = NextDownload();
                var obs = this.downloader.Download(d);
                DownloadStarted(this, d);
                obs.Subscribe(null, _ => CheckDownloadsToStart(), () => CheckDownloadsToStart());
            }
        }

        private IDownload NextDownload()
        {
            return this.Downloads.First();
        }

        private void OnDownloadStarted(IDownload e) => DownloadStarted?.Invoke(this, e);
    }
}