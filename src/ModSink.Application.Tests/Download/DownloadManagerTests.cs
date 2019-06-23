using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ModSink.Application.Download;
using Xunit;

namespace ModSink.Application.Tests.Download
{
    public class DownloadManagerTests
    {
        private class MockDownloader : IDownloader
        {
            public bool CanDownload(Uri uri)
            {
                return true;
            }

            public async Task<Stream> DownloadAsync(Uri uri, CancellationToken cancellationToken)
            {
                await Task.Delay(1, cancellationToken);
                return new MemoryStream(new byte[] {0x00});
            }
        }

        [Fact(Skip = "WIP")]
        public async Task Start()
        {
        }
    }
}