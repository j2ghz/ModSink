using System;
using System.IO;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using ModSink.Core.Client;
using ReactiveUI;

namespace ModSink.Common.Client
{
    public class Download : ReactiveObject, IDownload
    {
        private readonly Subject<DownloadProgress> progress = new Subject<DownloadProgress>();
        private DownloadState state = DownloadState.Queued;

        public Download(Uri source, Lazy<Task<Stream>> destination, string name)
        {
            Source = source;
            Destination = destination;
            Name = name;
        }

        public Lazy<Task<Stream>> Destination { get; }
        public string Name { get; }
        public IObservable<DownloadProgress> Progress => progress;
        public Uri Source { get; }

        public DownloadState State
        {
            get => state;
            private set => this.RaiseAndSetIfChanged(ref state, value);
        }

        public void Start(IDownloader downloader) //TODO: redo using Stateless
        {
            if (State != DownloadState.Queued)
                throw new Exception($"State must be {DownloadState.Queued} to start a download");
            State = DownloadState.Downloading;
            downloader.Download(this).Subscribe(progress);
            Progress.Subscribe(_ => { }, _ => State = DownloadState.Errored, () => State = DownloadState.Finished);
        }
    }
}