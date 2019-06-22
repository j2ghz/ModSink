using System.Collections.Generic;

namespace ModSink.Domain.Entities.Repo
{
    public class Modpack
    {
        public string Name { get; set; }
        public ICollection<Mod> Mods { get; set; }
    }
}