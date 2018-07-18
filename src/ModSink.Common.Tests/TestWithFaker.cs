using Bogus;
using FluentAssertions;
using Xunit;

namespace ModSink.Common.Tests
{
    public abstract class TestWithFaker<T> where T : class
    {
        public abstract Faker<T> Faker { get; }

        [Fact]
        [Trait("Category", "Serialization")]
        public void HasValidFaker()
        {
            Faker.AssertConfigurationIsValid();
        }


        [Fact]
        [Trait("Category", "Serialization")]
        public virtual void IsSerializeable()
        {
            for (var i = 0; i < 10; i++) Faker.Generate().Should().BeBinarySerializable();
        }
    }
}