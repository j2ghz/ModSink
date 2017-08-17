using System;
using System.Collections.Generic;
using System.Text;
using ModSink.Core.Models;

namespace Modsink.Common.Tests
{
    public class JsonSerializerTest : ISerializerTestsBase
    {
        protected override ISerializer serializer => new ModSink.Common.JsonSerializer();
    }
}