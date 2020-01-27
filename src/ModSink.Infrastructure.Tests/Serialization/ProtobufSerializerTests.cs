using System;
using System.Collections.Generic;
using System.Text;
using ModSink.Application.Serialization;
using ModSink.Application.Tests.Serialization;
using ModSink.Infrastructure.Serialization;
using ModSink.Infrastructure.Serialization.Protobuf;

namespace ModSink.Infrastructure.Tests.Serialization {
  public class ProtobufSerializerTests : IFormatterTests {
    protected override IFormatter formatter => new ProtobufSerializer();
  }
}
