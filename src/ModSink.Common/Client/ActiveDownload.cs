using System;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
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

        public ActiveDownload(in QueuedDownload source, Stream tempDestination, Action completed, IDownloader downloader)
        {
            Source = source.Source;
            Name = source.FileSignature.Hash.ToString();
            Destination = tempDestination;
            LogTo.Verbose("Created ActiveDownload for {signature}", source.FileSignature);
            var dProg = downloader.Download(Source, Destination, source.FileSignature.Length);
            dProg.Subscribe(progress).DisposeWith(disposable);
            dProg.Connect().DisposeWith(disposable);
            dProg.Subscribe(_ => { }, completed);
        }

        public string Name { get; }

        public Stream Destination { get; }
        public IObservable<DownloadProgress> Progress => progress;
        public Uri Source { get; }


        public void Dispose()
        {
            disposable?.Dispose();
            progress?.Dispose();
            Destination?.Dispose();
        }
    }
}