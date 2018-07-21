using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ModSink.Common.Client;
using Xunit;

namespace ModSink.Common.Tests
{
    public class HttpClientDownloaderTests
    {
        [Fact]
        public async Task Download()
        {
            var client = new HttpClientDownloader();
            using (var stream = new MemoryStream())
            {
                var obs = client.Download(new Uri(@"https://google.com/favicon.ico"), new Lazy<Stream>(()=> stream));
                using (var d = obs.Connect())
                {
                    var progress = await obs;
                    if (progress.Size.Bits != 0)
                    {
                        progress.Downloaded.Bits.Should().Be(progress.Size.Bits);
                        progress.Remaining.Bits.Should().Be(0);
                    }

                    progress.State.Should().Be(DownloadProgress.TransferState.Finished);
                }
            }
        }
    }
}