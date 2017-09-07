using ModSink.Core.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ModSink.Common.Client
{
    internal class Download : IDownload
    {
        public Download(Uri source, Lazy<Stream> destination)
        {
            Source = source;
            Destination = destination;
        }

        public Lazy<Stream> Destination { get; }

        public IObservable<DownloadProgress> Progress { get; private set; }

        public Uri Source { get; }
    }
}