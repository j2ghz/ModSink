using System;
using System.Collections.Generic;
using System.Text;
using ModSink.Application.Serialization;
using ModSink.Application.Tests.Serialization;
using ModSink.Infrastructure.Serialization;

namespace ModSink.Infrastructure.Tests.Serialization
{
    public class MsgPackCliSerializerTests : IFormatterTests
    {
        protected override IFormatter formatter => new MsgPackCliSerializer();
    }
}
