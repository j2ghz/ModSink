namespace ModSink.Core.Game
{
    public interface IGameLauncher
    {
        IGameConfig Configuration { get; set; }

        string Name { get; }
    }
}