using FluentAssertions;
using ModSink.Common.Models.Repo;
using Xunit;

namespace Modsink.Common.Tests.Models.Repo
{
    public class HashValueTests
    {
        [Fact]
        public void CreateFromToStringResult()
        {
            var hash = new HashValue(new byte[] {0x99, 0xE9, 0xD8, 0x51, 0x37, 0xDB, 0x46, 0xEF});
            var str = hash.ToString();
            var result = new HashValue(str);
            result.Should().BeEquivalentTo(hash);
        }
    }
}