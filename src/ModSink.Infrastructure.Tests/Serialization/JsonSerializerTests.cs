using ModSink.Application.Serialization;
using ModSink.Application.Tests.Serialization;
using ModSink.Infrastructure.Serialization;

namespace ModSink.Infrastructure.Tests.Serialization
{
    public class JsonSerializerTests : IFormatterTests
    {
        protected override IFormatter formatter => new JsonSerializer();
    }
}