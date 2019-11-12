using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ModSink.Application.Serialization;

namespace ModSink.Infrastructure.Serialization.Protobuf
{
    public class ProtobufSerializer : IFormatter
    {
        public Stream Serialize<T>(T o)
        {
            throw new NotImplementedException();
        }

        public void Serialize<T>(T o, Stream stream)
        {
            throw new NotImplementedException();
        }

        public T Deserialize<T>(Stream stream)
        {
            throw new NotImplementedException();
        }

        public bool CanDeserialize(string extension)
        {
            throw new NotImplementedException();
        }
    }
}
