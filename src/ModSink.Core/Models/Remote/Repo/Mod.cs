using ModSink.Core.Models.Local;
using System.Collections.Generic;

namespace ModSink.Core.Models.Remote.Repo
{
    public class Mod
    {
        public IDictionary<string, IHashValue> Files { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }
    }
}