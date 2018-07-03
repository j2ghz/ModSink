using System;
using System.Collections.Generic;
using System.Text;
using Bogus;
using FluentAssertions;
using Xunit;

namespace ModSink.Common.Tests
{
    public abstract class TestWithFaker<T> where T : class
    {
        public abstract Faker<T> Faker { get; }

        [Fact]
        public void HasValidFaker()
        {
            Faker.AssertConfigurationIsValid();
        }


        [Fact]
        public void IsSerializeable()
        {
            Assert.All(Faker.Generate(10), f => { f.Should().BeBinarySerializable(); });
        }
    }
}
