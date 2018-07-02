using System.Reactive.Linq;
using ModSink.Common.Client;
using Moq;
using Xunit;

namespace Modsink.Common.Tests.Client
{
    public class DownloadServiceTests
    {
        [Fact]
        public void AddDownload()
        {
            var ds = new DownloadService(null);
            var download = new Mock<Download>();
            var state = Download.DownloadState.Queued;
            download.Setup(d => d.State).Returns(() => state);
            download.Setup(d => d.Progress).Callback(() => state = Download.DownloadState.Downloading)
                .Returns(Observable.Empty<DownloadProgress>());

            ds.Add(download.Object);

            download.Verify(d => d.Start(null), Times.Once);
        }
    }
}