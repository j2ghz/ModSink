using Bogus;
using FluentAssertions;
using ModSink.Common.Models.Repo;
using Xunit;

namespace Modsink.Common.Tests.Models.Repo
{
    public class FileSignatureTests
    {
        private static readonly Faker Faker = new Faker();
        public static FileSignature FileSignature => new FileSignature(HashValueTests.HashValue, Faker.Random.ULong());

        [Fact]
        public void IsSerializeable()
        {
            for (var i = 0; i < 10; i++) FileSignature.Should().BeBinarySerializable();
        }
    }
}