namespace ModSink.Core.Models.Game
{
    public interface IGameLauncher<TConfig>
        where TConfig : IGameConfig
    {
        TConfig Configuration { get; set; }

        string Name { get; }
    }
}