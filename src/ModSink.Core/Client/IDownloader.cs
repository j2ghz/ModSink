using ModSink.Core.Client;
using System;
using System.IO;

namespace ModSink.Core.Local
{
    public interface IDownloader
    {
        IObservable<DownloadProgress> Download(Uri source, Stream destination);
    }
}