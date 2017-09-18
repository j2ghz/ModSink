using System;
using System.Collections.Generic;

namespace ModSink.Core.Models.Repo
{
    [Serializable]
    public class Modpack
    {
        public ICollection<ModEntry> Mods { get; set; }
        public string Name { get; set; }
        public ICollection<IServer> Servers { get; set; }
    }
}