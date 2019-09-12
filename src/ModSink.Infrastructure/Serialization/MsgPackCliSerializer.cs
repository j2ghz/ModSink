using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ModSink.Application.Serialization;
using MsgPack;
using MsgPack.Serialization;

namespace ModSink.Infrastructure.Serialization
{
    public class MsgPackCliSerializer : IFormatter
    {
        public Stream Serialize<T>(T o)
        {
            var stream = new MemoryStream();
            Serialize(o, stream);
            stream.Position = 0;
            return stream;
        }

        public void Serialize<T>(T o, Stream stream)
        {
            MessagePackSerializer.Get<T>().Pack(stream,o);
        }

        public T Deserialize<T>(Stream stream)
        {
            return MessagePackSerializer.Get<T>().Unpack(stream);
        }

        public bool CanDeserialize(string extension)
        {
            throw new NotImplementedException();
        }
    }
}
