using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Humanizer;
using Humanizer.Bytes;
using ModSink.Common.Client;
using Xunit;

namespace Modsink.Common.Tests
{
    public class MockDownloader : IDownloader
    {
        private readonly IDictionary<Uri, Stream> paths;


        public MockDownloader(IDictionary<Uri, Stream> paths)
        {
            this.paths = paths;
        }

        public IConnectableObservable<DownloadProgress> Download(Uri source, Stream destination,
            ulong expectedLength = 0)
        {
            return Observable.Create<DownloadProgress>(async (observer, cancel) =>
            {
                observer.OnNext(new DownloadProgress(0.Bits(), 0.Bits(), DownloadProgress.TransferState.NotStarted));
                observer.OnNext(new DownloadProgress(0.Bits(), 0.Bits(),
                    DownloadProgress.TransferState.AwaitingResponse));
                var stream = paths[source];
                observer.OnNext(new DownloadProgress(stream.Length.Bytes(), 0.Bits(),
                    DownloadProgress.TransferState.ReadingResponse));
                observer.OnNext(new DownloadProgress(stream.Length.Bytes(), 0.Bits(),
                    DownloadProgress.TransferState.Downloading));
                await stream.CopyToAsync(destination);

                var totalRead = 0;
                var buffer = new byte[16 * 1024];
                int read;
                
                observer.OnNext(new DownloadProgress(stream.Length.Bytes(), ByteSize.FromBytes(0),
                    DownloadProgress.TransferState.Downloading));
                while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, cancel)) > 0)
                {
                    destination.Write(buffer, 0, read);
                    totalRead += read;
                    observer.OnNext(new DownloadProgress(stream.Length.Bytes(), totalRead.Bytes(),
                        DownloadProgress.TransferState.Downloading));
                }

                observer.OnNext(new DownloadProgress(stream.Length.Bytes(), totalRead.Bytes(),
                    DownloadProgress.TransferState.Finished));
            }).Publish();
        }
    }
}