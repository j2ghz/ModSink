using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using Anotar.Serilog;
using Humanizer.Bytes;

namespace ModSink.Common.Client
{
    public class ActiveDownload :  IDisposable
    {
        private readonly CompositeDisposable disposable = new CompositeDisposable();

        private readonly BehaviorSubject<DownloadProgress> progress =
            new BehaviorSubject<DownloadProgress>(new DownloadProgress(ByteSize.FromBytes(0), ByteSize.FromBytes(0),
                DownloadProgress.TransferState.NotStarted));

        public ActiveDownload(IConnectableObservable<DownloadProgress> downloadProgress, Action completed, string name)
        {
            Name = name;
            LogTo.Verbose("[{download}] Created ActiveDownload", Name);
            disposable.Add(downloadProgress.Subscribe(progress));
            disposable.Add(downloadProgress.Connect());
            disposable.Add(progress.DistinctUntilChanged(dp => dp.State).Subscribe(dp =>
                LogTo.Verbose("[{download}] State changed to {state}", Name, dp.State)));
            disposable.Add(progress.Subscribe(_ => { }, completed));
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