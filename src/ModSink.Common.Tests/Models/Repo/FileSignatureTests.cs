using Bogus;
using FluentAssertions;
using FluentAssertions.Primitives;
using ModSink.Common.Models.Repo;
using Xunit;

namespace ModSink.Common.Tests.Models.Repo
{
    public class FileSignatureTests
    {
        private static readonly Faker Faker = new Faker();
        public static FileSignature FileSignature => new FileSignature(HashValueTests.HashValue, Faker.Random.ULong());

        [Fact]
        public void IsSerializeable()
        {
            for (var i = 0; i < 10; i++) new ObjectAssertions(FileSignature).BeBinarySerializable();
        }

        [Fact]
        public void SameEqual()
        {
            var a = new FileSignature(new HashValue(new byte[] {0x99, 0xE9, 0xD8, 0x51, 0x37, 0xDB, 0x46, 0xEF}), 355);
            var b = new FileSignature(new HashValue(new byte[] {0x99, 0xE9, 0xD8, 0x51, 0x37, 0xDB, 0x46, 0xEF}), 355);
            a.Equals(b).Should().BeTrue("FileSignature with same properties should be equal")
                ;
        }
    }
}