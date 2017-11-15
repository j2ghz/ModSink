using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Humanizer;
using Humanizer.Bytes;
using ModSink.Core.Client;
using ReactiveUI;
using Serilog;
using Stateless;

namespace ModSink.Common.Client
{
    public class Download : ReactiveObject, IDownload
    {
        private readonly BehaviorSubject<DownloadProgress> progress =
            new BehaviorSubject<DownloadProgress>(new DownloadProgress(ByteSize.FromBytes(0), ByteSize.FromBytes(0),
                DownloadProgress.TransferState.NotStarted));

        private readonly CompositeDisposable progressSubscription = new CompositeDisposable();

        private readonly StateMachine<DownloadState, Trigger> state;

        public Download(Uri source, Lazy<Task<Stream>> destination, string name)
        {
            Source = source;
            Destination = destination;
            Name = name;

            state = new StateMachine<DownloadState, Trigger>(DownloadState.Queued);
            state.OnTransitioned(_ => this.RaisePropertyChanged(nameof(State)));
            state.OnTransitioned(t =>
                log.Verbose("Download {name} switched to state {state}", Name, t.Destination.Humanize()));
            state.Configure(DownloadState.Queued)
                .Permit(Trigger.Start, DownloadState.Downloading);
            state.Configure(DownloadState.Downloading)
                .Permit(Trigger.Finish, DownloadState.Finished)
                .Permit(Trigger.Error, DownloadState.Errored)
                .OnDeactivate(() => progressSubscription.Dispose());
        }

        private ILogger log => Log.ForContext<Download>().ForContext("ID", Name);

        public Lazy<Task<Stream>> Destination { get; }
        public string Name { get; }
        public IObservable<DownloadProgress> Progress => progress;
        public Uri Source { get; }

        public DownloadState State => state.State;

        public async Task Start(IDownloader downloader)
        {
            state.Fire(Trigger.Start);
            var obs = downloader.Download(Source, await Destination.Value);
            progressSubscription.Add(obs.Connect());
            progressSubscription.Add(obs.Subscribe(progress));
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