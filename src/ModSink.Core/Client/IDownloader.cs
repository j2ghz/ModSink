using ModSink.Core.Client;
using System;
using System.IO;

namespace ModSink.Core.Client
{
    public interface IDownloader
    {
        IObservable<DownloadProgress> Download(Uri source, Stream destination);
    }
}