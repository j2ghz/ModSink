namespace ModSink.Core
{
    public interface IGameLauncher
    {
        IGameConfig Configuration { get; set; }

        string Name { get; }
    }
}