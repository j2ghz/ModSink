using System;
using System.IO;
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

        public ActiveDownload(in QueuedDownload source, DirectoryInfo tempDownloadsDirectory, IDownloader downloader)
        {
            Source = source.Source;
            Name = source.FileSignature.Hash.ToString();
            var tempFile = tempDownloadsDirectory.ChildFile(source.FileSignature.Hash.ToString());
            Destination = tempFile.Create();
            LogTo.Debug("Created ActiveDownload for {signature} into {destination}", source.FileSignature,tempFile);
            var dProg = downloader.Download(Source, Destination, source.FileSignature.Length);
            dProg.Subscribe(progress).DisposeWith(disposable);
            dProg.Connect().DisposeWith(disposable);
        }

        public string Name { get; }

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