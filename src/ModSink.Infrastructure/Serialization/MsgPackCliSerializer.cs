﻿using System;
using System.IO;
using ModSink.Application.Serialization;
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
            if (extension == null) throw new ArgumentNullException(nameof(extension));
            return extension.EndsWith("msgpack");
        }
    }
}