
namespace ModSink.Core.Models.Local
{
    public interface IManager
    {
        void LinkFile(IHash hash, string path);
    }
}