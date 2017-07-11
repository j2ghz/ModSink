using System;
using System.Collections.Generic;
using System.Text;

namespace ModSink.Core.Models.Online.Repo
{
    public class Mod
    {
        public IDictionary<string, byte[]> Files { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }
    }
}
