using System;
using System.Collections.Generic;
using System.Text;
using ModSink.Core.Client;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using ModSink.Common.Client;
using Humanizer.Bytes;
using Humanizer;
using static ModSink.Core.Client.DownloadProgress;

namespace ModSink.Common
{
    public class HttpClientDownloader : IDownloader
    {
        private HttpClient client = new HttpClient();

        public IObservable<DownloadProgress> Download(IDownload download)
        {
            var progress = Observable.Create<DownloadProgress>(async (observer, cancel) =>
            {
                var report = new Action<ByteSize, ByteSize, TransferState>((size, downloaded, state) => observer.OnNext(new DownloadProgress(size, downloaded, state)));
                //Get response
                report(ByteSize.FromBytes(0), ByteSize.FromBytes(0), TransferState.AwaitingResponse);
                var response = await this.client.GetAsync(download.Source, HttpCompletionOption.ResponseHeadersRead, cancel);

                //Read response
                report(ByteSize.FromBytes(0), ByteSize.FromBytes(0), TransferState.ReadingResponse);
                response.EnsureSuccessStatusCode();
                var length = ByteSize.FromBytes(response.Content.Headers.ContentLength.Value);
                report(length, ByteSize.FromBytes(0), TransferState.ReadingResponse);

                var totalRead = 0;
                using (var input = await response.Content.ReadAsStreamAsync())
                using (var output = download.Destination.Value)
                {
                    byte[] buffer = new byte[16 * 1024];
                    var read = 0;

                    //Download
                    report(length, ByteSize.FromBytes(0), TransferState.Downloading);
                    while ((read = await input.ReadAsync(buffer, 0, buffer.Length, cancel)) > 0)
                    {
                        await output.WriteAsync(buffer, 0, read);
                        totalRead += read;
                        report(length, totalRead.Bytes(), TransferState.Downloading);
                    }
                }

                //Finish
                report(length, totalRead.Bytes(), TransferState.Finished);
                observer.OnCompleted();
                return Disposable.Empty;
            }).Publish();

            progress.Connect();
            return progress;
        }

        public IObservable<DownloadProgress> Download(Uri source, Stream destination, string name) => this.Download(new Download(source, new Lazy<Stream>(() => destination), name));
    }
}