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
            var h1 = new TestHash(new byte[] { 0x00 }); var h2 = new TestHash(new byte[] { 0x00 });
            h1.Should().Be(h2);
        }
        class TestHash : Hash
        {
            public TestHash(byte[] value) : base(value)
            {
            }

            public override string HashId { get; } = "TestHash";
        }
    }
}