using ModSink.Core.Local;
using System;
using System.Collections.Generic;
using System.Text;
using ModSink.Core.Client;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace ModSink.Common
{
    public class HttpClientDownloader : IDownloader
    {
        private HttpClient client = new HttpClient();

        public IObservable<DownloadProgress> Download(Uri source, Stream destination)
        {
            return Observable.Create<DownloadProgress>(async (observer, cancel) =>
            {
                observer.OnNext(new DownloadProgress(0, 0, DownloadProgress.DownloadState.AwaitingResponse));
                var response = await this.client.GetAsync(source, HttpCompletionOption.ResponseHeadersRead, cancel);

                observer.OnNext(new DownloadProgress(0, 0, DownloadProgress.DownloadState.ReadingResponse));
                response.EnsureSuccessStatusCode();
                var length = (ulong)response.Content.Headers.ContentLength.Value;
                observer.OnNext(new DownloadProgress(length, 0, DownloadProgress.DownloadState.ReadingResponse));
                var input = await response.Content.ReadAsStreamAsync();

                var totalRead = 0UL;
                byte[] buffer = new byte[16 * 1024];
                var read = 0;

                observer.OnNext(new DownloadProgress(0, length, DownloadProgress.DownloadState.Downloading));
                while ((read = await input.ReadAsync(buffer, 0, buffer.Length, cancel)) > 0)
                {
                    await destination.WriteAsync(buffer, 0, read);
                    totalRead += (ulong)read;
                    observer.OnNext(new DownloadProgress(length, totalRead, DownloadProgress.DownloadState.Downloading));
                }
                observer.OnNext(new DownloadProgress(length, totalRead, DownloadProgress.DownloadState.Finished));

                observer.OnCompleted();

                return Disposable.Empty;
            });
        }
    }
}