using System;
using System.Collections.Generic;
using System.Text;

namespace ModSink.Core.Client
{
    public interface IDownloadManager
    {
        event EventHandler<IDownload> DownloadStarted;

        IList<IDownload> Downloads { get; }

        void CheckDownloadsToStart();
    }
}