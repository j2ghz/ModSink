using System;
using System.IO;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Humanizer;
using Humanizer.Bytes;
using ModSink.Common.Client;
using ModSink.Core.Client;
using static ModSink.Core.Client.DownloadProgress;

namespace ModSink.Common
{
    public class HttpClientDownloader : IDownloader
    {
        private readonly HttpClient client = new HttpClient();

        public IConnectableObservable<DownloadProgress> Download(Uri source, Stream destination, ulong expectedLength = 0)
        {
            return Observable.Create<DownloadProgress>(async (observer, cancel) =>
            {
                try
                {
                    var report = new Action<ByteSize, ByteSize, TransferState>((size, downloaded, state) =>
                        observer.OnNext(new DownloadProgress(size, downloaded, state)));
                    //Get response
                    report(ByteSize.FromBytes(0), ByteSize.FromBytes(0), TransferState.AwaitingResponse);
                    var response = await client.GetAsync(source, HttpCompletionOption.ResponseHeadersRead,
                        cancel);

                    //Read response
                    report(ByteSize.FromBytes(0), ByteSize.FromBytes(0), TransferState.ReadingResponse);
                    response.EnsureSuccessStatusCode();
                    if (expectedLength != 0)
                    {
                        if (response.Content.Headers.ContentLength > 0)
                        {
                            if (Convert.ToUInt64(response.Content.Headers.ContentLength) != expectedLength)
                            {
                                throw new HttpRequestException($"Size of the download ({response.Content.Headers.ContentLength}) differed from expected ({expectedLength})");
                            }
                        }
                    }
                    var length = ByteSize.FromBytes(response.Content.Headers.ContentLength ?? 0);
                    
                    report(length, ByteSize.FromBytes(0), TransferState.ReadingResponse);

                    var totalRead = 0;
                    using (var input = await response.Content.ReadAsStreamAsync())
                    using (var output = destination)
                    {
                        var buffer = new byte[16 * 1024];
                        var read = 0;

                        //Download
                        report(length, ByteSize.FromBytes(0), TransferState.Downloading);
                        while ((read = await input.ReadAsync(buffer, 0, buffer.Length, cancel)) > 0)
                        {
                            output.Write(buffer, 0, read);
                            totalRead += read;
                            report(length, totalRead.Bytes(), TransferState.Downloading);
                        }
                    }

                    //Finish
                    report(length, totalRead.Bytes(), TransferState.Finished);
                    observer.OnCompleted();
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                }

                return Disposable.Empty;
            }).Publish();
        }
    }
}