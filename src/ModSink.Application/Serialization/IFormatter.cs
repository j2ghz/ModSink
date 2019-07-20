using System.IO;

namespace ModSink.Application.Serialization
{
    public interface IFormatter
    {
        Stream Serialize<T>(T o);
        void Serialize<T>(T o, Stream stream);
        T Deserialize<T>(Stream stream);
        bool CanDeserialize(string extension);
    }
}