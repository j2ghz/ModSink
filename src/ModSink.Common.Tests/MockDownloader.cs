using System;
using System.IO;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer.Bytes;
using ModSink.Common.Client;
using System.Reactive.Subjects;

namespace Modsink.Common.Tests
{
    public class MockDownloader : IDownloader
    {
        private readonly bool shouldFail;
        private readonly bool slow;

        public MockDownloader(bool shouldFail, bool slow)
        {
            this.shouldFail = shouldFail;
            this.slow = slow;
        }

        public IConnectableObservable<DownloadProgress> Download(Uri source, Stream destination, ulong expectedLength = 0)
        {
            return Observable.Create<DownloadProgress>(async (observer,cancel) =>
            {
                //Get response
                observer.OnNext(new DownloadProgress(
                    ByteSize.FromBytes(0),
                    ByteSize.FromBytes(0),
                    DownloadProgress.TransferState.AwaitingResponse));

                //Read response
                observer.OnNext(new DownloadProgress(
                    ByteSize.FromBytes(0),
                    ByteSize.FromBytes(0),
                    DownloadProgress.TransferState.ReadingResponse));

                observer.OnNext(new DownloadProgress(
                    ByteSize.FromBytes(1),
                    ByteSize.FromBytes(0),
                    DownloadProgress.TransferState.ReadingResponse));

                //Download
                observer.OnNext(new DownloadProgress(
                    ByteSize.FromBytes(1),
                    ByteSize.FromBytes(0),
                    DownloadProgress.TransferState.Downloading));

                if (slow)
                {
                    for (var i = 0; i < 100; i++)
                    {
                        observer.OnNext(new DownloadProgress(
                            ByteSize.FromBytes(100),
                            ByteSize.FromBytes(i),
                            DownloadProgress.TransferState.Downloading));
                        await Task.Delay(10,cancel);
                    }
                }

                if (shouldFail) throw new HttpRequestException();
                //Finish
                observer.OnNext(new DownloadProgress(
                    ByteSize.FromBytes(1),
                    ByteSize.FromBytes(1),
                    DownloadProgress.TransferState.Finished));

                observer.OnCompleted();
                return Disposable.Empty;
            }).Publish();
        }
    }
}