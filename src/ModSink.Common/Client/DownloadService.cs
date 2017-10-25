using ModSink.Core.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using DynamicData;

namespace ModSink.Common.Client
{
    public class DownloadService : IDownloadService
    {
        private readonly IDownloader downloader;
        private readonly SourceList<IDownload> downloads = new SourceList<IDownload>();
        private byte simultaneousDownloads = 1;

        public DownloadService(IDownloader downloader)
        {
            this.downloader = downloader;
        }

        public IObservableList<IDownload> Queue => this.downloads.AsObservableList();

        public void Add(IDownload download)
        {
            this.downloads.Add(download);
            CheckDownloadsToStart();
        }

        public void CheckDownloadsToStart()
        {
            var toStart = this.simultaneousDownloads - this.downloads.Items.Count(d => d.State == DownloadState.Downloading);
            for (int i = 0; i < toStart; i++)
            {
                var d = NextDownload();
                if (d == null) break;
                d.Start(this.downloader);
                d.Progress.Subscribe(_ => { }, _ => CheckDownloadsToStart(), () => CheckDownloadsToStart());
            }
        }

        private IDownload NextDownload()
        {
            return this.Queue.Items.Where(d => d.State == DownloadState.Queued).FirstOrDefault();
        }
    }
}