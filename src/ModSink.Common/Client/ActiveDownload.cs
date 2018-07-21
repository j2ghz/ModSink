using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Anotar.Serilog;
using Humanizer.Bytes;
using ReactiveUI;

namespace ModSink.Common.Client
{
    public class ActiveDownload : ReactiveObject, IDisposable
    {
        private readonly CompositeDisposable disposable = new CompositeDisposable();

        private readonly BehaviorSubject<DownloadProgress> progress =
            new BehaviorSubject<DownloadProgress>(new DownloadProgress(ByteSize.FromBytes(0), ByteSize.FromBytes(0),
                DownloadProgress.TransferState.NotStarted));

        public ActiveDownload(in QueuedDownload source, Lazy<Stream> destination, Action completed,
            IDownloader downloader)
        {
            Source = source.Source;
            Name = source.FileSignature.Hash.ToString();
            LogTo.Debug("Created ActiveDownload for {signature}", source.FileSignature);
            var dProg = downloader.Download(Source, destination, source.FileSignature.Length);
            dProg.Subscribe(progress).DisposeWith(disposable);
            dProg.Connect().DisposeWith(disposable);

            progress.DistinctUntilChanged(dp => dp.State).Subscribe(dp =>
                LogTo.Verbose("[{download}] State changed to {state}", Name, dp.State));

            progress.Subscribe(_ => { }, () =>
            {
                destination.Value?.Dispose();
                completed();
                this.Dispose();
            });
        }

        public string Name { get; }
        public IObservable<DownloadProgress> Progress => progress;
        public Uri Source { get; }


        public void Dispose()
        {
            LogTo.Debug("Removed ActiveDownload for {signature}", Name);
            disposable?.Dispose();
            progress?.Dispose();
        }
    }
}