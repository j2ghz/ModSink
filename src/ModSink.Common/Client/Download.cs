using ModSink.Core.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace ModSink.Common.Client
{
    public class Download : IDownload
    {
        public Download(Uri source, Lazy<Stream> destination)
        {
            this.Source = source;
            this.Destination = destination;
        }

        public Lazy<Stream> Destination { get; }
        public IObservable<DownloadProgress> Progress { get; private set; }
        public Uri Source { get; }
        public DownloadState State { get; private set; } = DownloadState.Stopped;

        public void Start(IDownloader downloader)
        {
            this.State = DownloadState.Downloading;
            this.Progress = downloader.Download(this.Source, this.Destination.Value);
            this.Progress.Subscribe(null, _ => this.State = DownloadState.Errored, () => this.State = DownloadState.Finished);
        }
    }
}