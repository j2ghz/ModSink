using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using ModSink.Common;
using Xunit;

namespace Modsink.Common.Tests
{
    public class HashingTests
    {
        [Fact]
        public async Task GetHashOfEmptyAsync()
        {
            var hashF = new XXHash64();
            var hashing = new HashingService(hashF);
            var stream = new MemoryStream(new byte[] { });
            var hash = await hashing.GetFileHash(stream, CancellationToken.None);
            hash.Should().Be(hashF.HashOfEmpty);
        }
    }
}