using System.Diagnostics;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ModSink.Common.Client;
using ModSink.Core.Client;
using ReactiveUI;
using Xunit;

namespace Modsink.Common.Tests.Client
{
    public class DownloadTests : ReactiveObject
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
            download.Progress.Subscribe(Observer.Create<DownloadProgress>(dp => Trace.Write(dp.Remaining)));
            download.Start(new MockDownloader(false, true));
            var final = await download.Progress;
            Assert.Equal(DownloadState.Finished, DownloadState.Finished);
            Assert.Equal(DownloadProgress.TransferState.Finished, final.State);
        }
    }
}