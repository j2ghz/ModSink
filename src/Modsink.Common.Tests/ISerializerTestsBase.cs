using ModSink.Core.Models;
using ModSink.Core.Models.Repo;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Modsink.Common.Tests
{
    public abstract class ISerializerTestsBase
    {
        protected abstract ISerializer serializer { get; }

        [Fact]
        public void DeserializeSampleRepo()
        {
            var serialized = serializer.Serialize(TestDataBuilder.Repo());
            var deserialized = serializer.Deserialize(serialized);
            Assert.NotNull(deserialized);
        }

        [Fact]
        public void SerializeSampleRepo()
        {
            Assert.NotNull(serializer.Serialize(TestDataBuilder.Repo()));
        }
    }
}