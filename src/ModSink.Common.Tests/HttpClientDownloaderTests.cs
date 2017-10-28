using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Humanizer.Bytes;
using Microsoft.Reactive.Testing;
using ModSink.Common;
using ModSink.Core.Client;
using Xunit;

namespace Modsink.Common.Tests
{
    public class HttpClientDownloaderTests
    {
        [Fact]
        public async Task Download()
        {
            var client = new HttpClientDownloader();
            using (var stream = new MemoryStream())
            {
                var obs = client.Download(new Uri(@"http://ipv4.download.thinkbroadband.com/5MB.zip"), stream, "TestDownload");
                var progress = await obs;
                progress.Downloaded.ShouldBeEquivalentTo(progress.Size);
                progress.Remaining.ShouldBeEquivalentTo(ByteSize.FromBits(0));
                progress.State.ShouldBeEquivalentTo(DownloadProgress.TransferState.Finished);
            }
        }

        
    }
}