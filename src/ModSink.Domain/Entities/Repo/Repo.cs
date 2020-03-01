using System.Collections.Generic;

namespace ModSink.Domain.Entities.Repo
{
    [Equals]
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
        public static bool operator ==(Repo left, Repo right) => Operator.Weave(left, right);
        public static bool operator !=(Repo left, Repo right) => Operator.Weave(left, right);
    }
}
