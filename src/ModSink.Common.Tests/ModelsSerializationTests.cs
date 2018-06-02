using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using FluentAssertions;
using ModSink.Core.Models.Repo;
using Xunit;

namespace Modsink.Common.Tests
{
    public class ModelsSerializationTests
    {
        private readonly IFormatter formatter = new BinaryFormatter();

        [Fact]
        public void SerializeDeserializeRepo()
        {
            var repo = TestDataBuilder.Repo();
            var stream = new MemoryStream();
            formatter.Serialize(stream, repo);
            stream.Length.Should().BeGreaterThan(0);
            stream.Position = 0;
            var obj = formatter.Deserialize(stream);
            Assert.IsAssignableFrom<Repo>(obj);
        }
    }
}