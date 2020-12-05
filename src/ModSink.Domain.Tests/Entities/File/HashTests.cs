using FluentAssertions;
using ModSink.Domain.Entities.File;
using Xunit;

namespace ModSink.Domain.Tests.Entities.File
{
    public class HashTests
    {
        [Fact]
        public void SameEquals()
        {
            var h1 = new Hash("test", new byte[] { 0x00 });
            var h2 = new Hash("test", new byte[] { 0x00 });
            h1.Should().BeEquivalentTo(h2);
            //h1.Should().Be(h2);
        }
    }
}
