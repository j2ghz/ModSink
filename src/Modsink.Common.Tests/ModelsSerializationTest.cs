using ModSink.Core.Models.Repo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Xunit;
using FluentAssertions;

namespace Modsink.Common.Tests
{
    public class ModelsSerializationTest
    {
        private IFormatter formatter = new BinaryFormatter();

        [Fact]
        public void DeserializeRepo()
        {
            var stream = SerializeRepo();
            stream.Position = 0;
            var obj = formatter.Deserialize(stream);
            Assert.IsAssignableFrom<Repo>(obj);
        }

        [Fact]
        public Stream SerializeRepo()
        {
            var repo = TestDataBuilder.Repo();
            var stream = new MemoryStream();
            this.formatter.Serialize(stream, repo);
            stream.Length.Should().BeGreaterThan(0);
            return stream;
        }
    }
}