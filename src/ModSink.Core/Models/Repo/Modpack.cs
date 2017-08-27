using System;
using System.Collections.Generic;

namespace ModSink.Core.Models.Repo
{
    [Serializable]
    public class Modpack
    {
        public IEnumerable<ModEntry> Mods { get; set; }
        public string Name { get; set; }
        public IEnumerable<IServer> Servers { get; set; }
    }
}