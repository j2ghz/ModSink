using System.Collections.Generic;
using System.Text;

namespace ModSink.Core.Models.Online.Repo
{
    public class Modpack
    {
        public IDictionary<Mod,ModFlags> Mods { get; set; }

        public IEnumerable<IServer> Servers { get; set; }
    }
}
