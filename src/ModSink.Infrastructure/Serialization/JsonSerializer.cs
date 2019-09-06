using System;
using System.IO;
using System.Text;
using ModSink.Application.Serialization;
using Newtonsoft.Json;

namespace ModSink.Infrastructure.Serialization
{
    public class JsonSerializer : Newtonsoft.Json.JsonSerializer, IFormatter
    {
        public JsonSerializer()
        {
            TypeNameHandling = TypeNameHandling.All;
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full;
        }

        public Stream Serialize<T>(T o)
        {
            var stream = new MemoryStream();
            Serialize(o, stream);
            stream.Position = 0;
            return stream;
        }

        public void Serialize<T>(T o, Stream stream)
        {
            using var sw = new StreamWriter(stream, Encoding.UTF8, 10 * 1024, true);
            using var jw = new JsonTextWriter(sw);
            Serialize(jw, o);
        }

        public T Deserialize<T>(Stream stream)
        {
            using var sr = new StreamReader(stream, Encoding.UTF8);
            using var reader = new JsonTextReader(sr);
            return Deserialize<T>(reader);
        }

        public bool CanDeserialize(string extension)
        {
            if (extension == null) throw new ArgumentNullException(nameof(extension));
            return extension.EndsWith("json");
        }
    }
}