namespace ModSink.Core.Models.Game
{
    public interface IGameLauncher
    {
        IGameConfig Configuration { get; set; }

        string Name { get; }
    }
}