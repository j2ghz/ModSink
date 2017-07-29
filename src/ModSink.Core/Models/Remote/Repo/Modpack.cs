using System.Collections.Generic;

namespace ModSink.Core.Models.Remote.Repo
{
    public class Modpack
    {
        public IDictionary<Mod, ModFlags> Mods { get; set; }

        public IEnumerable<IServer> Servers { get; set; }
    }
}