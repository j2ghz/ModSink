using System;
using System.Collections.Generic;
using System.Text;

namespace ModSink.Domain.Entities.Repo
{
    public class Modpack
    {
        public string Name { get; private set; }
        public ICollection<Mod> Mods { get; private set; }
    }
}
