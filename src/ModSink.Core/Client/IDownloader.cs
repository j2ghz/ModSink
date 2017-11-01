using ModSink.Core.Client;
using System;
using System.IO;
using System.Reactive.Subjects;

namespace ModSink.Core.Client
{
    public interface IDownloader
    {
        IConnectableObservable<DownloadProgress> Download(Uri source, Stream destination, string name);

        IConnectableObservable<DownloadProgress> Download(IDownload download);
    }
}