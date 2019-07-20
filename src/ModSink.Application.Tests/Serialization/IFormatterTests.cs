using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using ModSink.Application.Serialization;
using Xunit;

namespace ModSink.Application.Tests.Serialization
{
    public abstract class IFormatterTests
    {
        protected abstract IFormatter formatter { get; }

        [Theory]
        [InlineData("a")]
        [InlineData(1)]
        public void Roundtrip(object o)
        {
            var stream = formatter.Serialize(o);
            formatter.Deserialize<object>(stream).Should().BeEquivalentTo(o);
        }

        [Fact]
        public void RoundtripFsCheck()
        {
            Prop.ForAll<object>(o =>
            {
                var stream = formatter.Serialize(o);
                formatter.Deserialize<object>(stream).Should().BeEquivalentTo(o);
                return true;
            }).QuickCheckThrowOnFailure();
            
        }
    }
}