using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
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

        [Fact]
        public async Task Start()
        {
            var dm = new DownloadManager(new[] {new MockDownloader()});
            for (var i = 0; i < 5; i++) dm.Add(new Uri("", UriKind.Relative));

            dm.QueueSize.Should().Be(5);
            dm.Start();
            dm.Stop();
            throw new NotImplementedException();
        }
    }
}