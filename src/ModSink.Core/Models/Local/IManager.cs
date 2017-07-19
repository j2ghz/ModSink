namespace ModSink.Core.Models.Local
{
    public interface IManager
    {
        void LinkFile(IHashValue hash, string path);
    }
}