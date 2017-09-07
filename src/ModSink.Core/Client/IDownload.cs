using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ModSink.Core.Client
{
    public interface IDownload
    {
        Lazy<Stream> Destination { get; }
        IObservable<DownloadProgress> Progress { get; }
        Uri Source { get; }
    }
}