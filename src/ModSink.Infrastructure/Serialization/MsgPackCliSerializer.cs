using System.IO;
using ModSink.Application.Serialization;
using MsgPack.Serialization;

namespace ModSink.Infrastructure.Serialization
{
    public class MsgPackCliSerializer : GenericFormatter
    {
        public override T Deserialize<T>(Stream stream)
        {
            return MessagePackSerializer.Get<T>().Unpack(stream);
        }

        public override Stream Serialize<T>(T o)
        {
            var stream = new MemoryStream();
            Serialize(o, stream);
            stream.Position = 0;
            return stream;
        }

        public void Serialize<T>(T o, Stream stream)
        {
            MessagePackSerializer.Get<T>().Pack(stream, o);
        }
    }
}