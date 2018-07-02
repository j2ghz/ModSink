using System;
using System.Linq;
using System.Net;
using Anotar.Serilog;
using DynamicData;

namespace ModSink.Common.Client
{
    public class DownloadService
    {
        private readonly IDownloader downloader;
        private readonly SourceCache<Download, Guid> downloads = new SourceCache<Download, Guid>(d => d.Id);
        private byte simultaneousDownloads;

        public DownloadService(IDownloader downloader)
        {
            this.downloader = downloader;
            SimultaneousDownloads = 5;
        }

        public byte SimultaneousDownloads
        {
            get => simultaneousDownloads;
            set
            {
                simultaneousDownloads = value;
                ServicePointManager.DefaultConnectionLimit = value;
            }
        }

        public IObservableCache<Download, Guid> Downloads => downloads.AsObservableCache();

        public void Add(Download download)
        {
            LogTo.Debug("Scheduling {download}", download);
            downloads.AddOrUpdate(download);
            CheckDownloadsToStart();
        }

        private void CheckDownloadsToStart()
        {
            var toStart = SimultaneousDownloads -
                          downloads.Items.Count(d => d.State == Download.DownloadState.Downloading);
            for (var i = 0; i < toStart; i++)
            {
                var d = NextDownload();
                if (d == null) break;
                d.Start(downloader);
                d.Progress.Subscribe(_ => { }, _ => CheckDownloadsToStart(), CheckDownloadsToStart);
            }
        }

        private Download NextDownload()
        {
            return Downloads.Items.FirstOrDefault(d => d.State == Download.DownloadState.Queued);
        }
    }
}