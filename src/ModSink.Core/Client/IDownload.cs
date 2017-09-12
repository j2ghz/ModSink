using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ModSink.Core.Client
{
    public enum DownloadState
    {
        Stopped,
        Downloading,
        Errored,
        Finished
    }

    public interface IDownload
    {
        Lazy<Stream> Destination { get; }
        IObservable<DownloadProgress> Progress { get; }
        Uri Source { get; }
        DownloadState State { get; }

        void Start(IDownloader downloader);
    }
}