using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ModSink.Core.Client
{
    public enum DownloadState
    {
        Queued,
        Downloading,
        Errored,
        Finished
    }

    public interface IDownload
    {
        Lazy<Stream> Destination { get; }
        string Name { get; }
        IObservable<DownloadProgress> Progress { get; }
        Uri Source { get; }
        DownloadState State { get; }

        void Start(IDownloader downloader);
    }
}