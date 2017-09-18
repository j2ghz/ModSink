using System;
using System.Collections.Generic;
using System.Text;

namespace ModSink.Core.Client
{
    public interface IDownloadManager
    {
        event EventHandler<IDownload> DownloadStarted;

        ICollection<IDownload> Downloads { get; }

        void CheckDownloadsToStart();
    }
}