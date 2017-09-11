using ModSink.Core.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModSink.Common.Client
{
    public class DownloadManager : IDownloadManager
    {
        private readonly IDownloader downloader;

        public DownloadManager(IDownloader downloader)
        {
            this.downloader = downloader;
        }

        public ICollection<IDownload> Downloads => new List<IDownload>();
    }
}