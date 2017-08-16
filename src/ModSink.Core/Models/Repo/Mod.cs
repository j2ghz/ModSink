using System.Collections.Generic;

namespace ModSink.Core.Models.Repo
{
    public class Mod
    {
        public IDictionary<string, IHashValue> Files { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }
    }
}