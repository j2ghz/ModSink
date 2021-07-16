using System.Collections.Generic;

namespace ModSink.Domain.Entities.Repo
{
    public class Repo
    {
        public Repo(string name, IReadOnlyCollection<Modpack> modpacks, string chunksPath)
        {
            Name = name;
            Modpacks = modpacks;
            ChunksPath = chunksPath;
        }

        public string ChunksPath { get; }

        public IReadOnlyCollection<Modpack> Modpacks { get; }
        public string Name { get; }
    }
}
