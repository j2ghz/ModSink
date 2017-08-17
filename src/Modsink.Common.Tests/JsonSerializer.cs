using System;
using System.Collections.Generic;
using System.Text;
using ModSink.Core.Models;

namespace Modsink.Common.Tests
{
    internal class JsonSerializer : ISerializerTestsBase
    {
        protected override ISerializer serializer => new ModSink.Common.JsonSerializer();
    }
}