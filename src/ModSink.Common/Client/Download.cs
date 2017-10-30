using System;
using System.IO;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Humanizer;
using ModSink.Core.Client;
using ReactiveUI;
using Serilog;
using Stateless;

namespace ModSink.Common.Client
{
    public class Download : ReactiveObject, IDownload
    {
        private readonly ILogger log = Log.ForContext<Download>();
        private readonly Subject<DownloadProgress> progress = new Subject<DownloadProgress>();
        private readonly StateMachine<DownloadState, Trigger> state;

        public Download(Uri source, Lazy<Task<Stream>> destination, string name)
        {
            Source = source;
            Destination = destination;
            Name = name;

            state = new StateMachine<DownloadState, Trigger>(DownloadState.Queued);
            state.OnTransitioned(_ => this.RaisePropertyChanged(nameof(State)));
            state.OnTransitioned(t =>
                Log.Verbose("Download {name} switched to state {state}", Name, t.Destination.Humanize()));
            state.Configure(DownloadState.Queued)
                .Permit(Trigger.Start, DownloadState.Downloading);
            state.Configure(DownloadState.Downloading)
                .Permit(Trigger.Finish, DownloadState.Finished)
                .Permit(Trigger.Error, DownloadState.Errored);
        }

        public Lazy<Task<Stream>> Destination { get; }
        public string Name { get; }
        public IObservable<DownloadProgress> Progress => progress;
        public Uri Source { get; }

        public DownloadState State => state.State;

        public void Start(IDownloader downloader) //TODO: redo using Stateless
        {
            state.Fire(Trigger.Start);
            downloader.Download(this).Subscribe(progress);
            Progress.Subscribe(_ => { }, _ => state.Fire(Trigger.Error), () => state.Fire(Trigger.Finish));
        }

        private enum Trigger
        {
            Start,
            Finish,
            Error
        }
    }
}