using System;
using System.Linq;
using DynamicData;
using ModSink.Core.Client;

namespace ModSink.Common.Client
{
    public class DownloadService : IDownloadService
    {
        private readonly IDownloader downloader;
        private readonly SourceList<IDownload> downloads = new SourceList<IDownload>();

        public DownloadService(IDownloader downloader)
        {
            this.downloader = downloader;
        }

        public byte SimultaneousDownloads { get; set; } = 5;

        public IObservableList<IDownload> Downloads => downloads.AsObservableList();

        public void Add(IDownload download)
        {
            downloads.Add(download);
            CheckDownloadsToStart();
        }

        private void CheckDownloadsToStart()
        {
            var toStart = SimultaneousDownloads - downloads.Items.Count(d => d.State == DownloadState.Downloading);
            for (var i = 0; i < toStart; i++)
            {
                var d = NextDownload();
                if (d == null) break;
                d.Start(downloader);
                d.Progress.Subscribe(_ => { }, _ => CheckDownloadsToStart(), CheckDownloadsToStart);
            }
        }

        private IDownload NextDownload()
        {
            return Downloads.Items.FirstOrDefault(d => d.State == DownloadState.Queued);
        }
    }
}