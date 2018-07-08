using Bogus;
using FluentAssertions;
using ModSink.Common.Models.Repo;
using Xunit;

namespace ModSink.Common.Tests.Models.Repo
{
    public class HashValueTests
    {
        private static readonly Faker Faker = new Faker();
        public static HashValue HashValue => new HashValue(Faker.Random.Bytes(8));

        [Fact]
        public void CreateFromToStringResult()
        {
            var hash = new HashValue(new byte[] {0x99, 0xE9, 0xD8, 0x51, 0x37, 0xDB, 0x46, 0xEF});
            var str = hash.ToString();
            var result = new HashValue(str);
            result.Should().BeEquivalentTo(hash);
        }

        [Fact]
        public void IsSerializeable()
        {
            for (var i = 0; i < 10; i++) HashValue.Should().BeBinarySerializable();
        }
    }
}