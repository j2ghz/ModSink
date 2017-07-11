using System;
using System.Collections.Generic;
using System.Text;

namespace ModSink.Core.Models.Online.Repo
{
    [Flags]
    public enum ModFlags
    {
        Optional = 1,
        DefaultOff = 2
    }
    public class Modpack
    {
        public IDictionary<Mod,ModFlags> Mods { get; set; }
    }
}
