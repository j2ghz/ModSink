using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ModSink.Common.Client;
using ModSink.Core.Client;
using Xunit;

namespace Modsink.Common.Tests.Client
{
    public class DownloadTests
    {
        [Fact]
        public async Task StartAndFailAsync()
        {
            var download = new Download(null, null, null);
            Assert.Equal(download.State, DownloadState.Queued);
            download.Start(new MockDownloader(true, false));
            await Assert.ThrowsAsync<HttpRequestException>(async () => await download.Progress);
        }

        [Fact]
        public async Task StartAndFinishAsync()
        {
            var download = new Download(null, null, null);
            Assert.Equal(download.State, DownloadState.Queued);
            download.Start(new MockDownloader(false, true));
            var final = await download.Progress;
            final.State.ShouldBeEquivalentTo(DownloadState.Finished);
        }
    }
}