using System;
using System.IO;
using System.Text;
using ModSink.Application.Serialization;
using Newtonsoft.Json;

namespace ModSink.Infrastructure.Serialization
{
    public class JsonSerializer : GenericFormatter
    {
        private readonly Newtonsoft.Json.JsonSerializer Serializer = new Newtonsoft.Json.JsonSerializer();

        public override T Deserialize<T>(Stream stream)
        {
            using var sr = new StreamReader(stream, Encoding.UTF8);
            using var reader = new JsonTextReader(sr);
            return Serializer.Deserialize<T>(reader);
        }

        public override Stream Serialize<T>(T o)
        {
            var stream = new MemoryStream();
            Serializer.Serialize(new StreamWriter(stream, Encoding.Default, 0, true), o);
            stream.Position = 0;
            return stream;
        }
    }
}