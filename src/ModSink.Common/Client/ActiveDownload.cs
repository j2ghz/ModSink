using System;
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

        public ActiveDownload(IConnectableObservable<DownloadProgress> downloadProgress, Action completed, string name)
        {
            Name = name;
            LogTo.Verbose("[{download}] Created ActiveDownload", Name);
            downloadProgress.Subscribe(progress).DisposeWith(disposable);
            downloadProgress.Connect().DisposeWith(disposable);
            progress.DistinctUntilChanged(dp => dp.State).Subscribe(dp =>
                LogTo.Verbose("[{download}] State changed to {state}", Name, dp.State)).DisposeWith(disposable);
            progress.Subscribe(_ => { }, () =>
            {
                completed();
                Dispose();
            }).DisposeWith(disposable);
        }

        public string Name { get; }
        public IObservable<DownloadProgress> Progress => progress;

        public void Dispose()
        {
            LogTo.Verbose("[{download}] Removed ActiveDownload", Name);
            disposable?.Dispose();
            progress?.Dispose();
        }
    }
}