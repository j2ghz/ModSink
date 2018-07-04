using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Humanizer.Bytes;
using ReactiveUI;

namespace ModSink.Common.Client
{
    public partial class Download : ReactiveObject, IDisposable
    {
        private readonly BehaviorSubject<DownloadProgress> progress =
            new BehaviorSubject<DownloadProgress>(new DownloadProgress(ByteSize.FromBytes(0), ByteSize.FromBytes(0),
                DownloadProgress.TransferState.NotStarted));

        private readonly CompositeDisposable disposable = new CompositeDisposable();

        public Download(in QueuedDownload source, DirectoryInfo tempDownloadsDirectory, IDownloader downloader)
        {
            Source = source.Source;
            Destination = tempDownloadsDirectory.ChildFile(source.FileSignature.Hash.ToString()).Create();
            var dProg = downloader.Download(Source, Destination, source.FileSignature.Length);
            dProg.Subscribe(progress).DisposeWith(disposable);
            dProg.Connect().DisposeWith(disposable);
        }

        public FileStream Destination { get; }
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