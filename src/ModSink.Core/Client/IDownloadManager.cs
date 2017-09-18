using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ModSink.Core.Client
{
    public interface IDownloadManager
    {
        event EventHandler<IDownload> DownloadStarted;

        ObservableCollection<IDownload> Downloads { get; }

        void CheckDownloadsToStart();
    }
}