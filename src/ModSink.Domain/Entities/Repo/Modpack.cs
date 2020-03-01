using System.Collections.Generic;

namespace ModSink.Domain.Entities.Repo
{
    public class Modpack
    {
        public IReadOnlyCollection<Mod> Mods { get; set; }
        public string Name { get; set; }
    }
}
