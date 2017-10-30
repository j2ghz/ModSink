using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
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
            download.Start(new MockDownloader(true));
            await Assert.ThrowsAsync<HttpRequestException>(async () => await download.Progress);
        }

        [Fact(Skip = "completes too fast, await misses all elements")]
        public async Task StartAndFinishAsync()
        {
            var download = new Download(null, null, null);
            Assert.Equal(download.State, DownloadState.Queued);
            download.Start(new MockDownloader(false));
            await download.Progress;
        }
    }
}