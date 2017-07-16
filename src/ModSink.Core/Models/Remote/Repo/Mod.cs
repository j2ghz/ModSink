using System.Collections.Generic;

namespace ModSink.Core.Models.Remote.Repo
{
    public class Mod
    {
        public IDictionary<string, IHash> Files { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }
    }
}