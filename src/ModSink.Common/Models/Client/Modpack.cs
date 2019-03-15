using System;
using System.Collections.Generic;
using System.Text;
using ModSink.Common.Models.DTO.Repo;

namespace ModSink.Common.Models.Client
{
    public class Modpack
    {
        public ICollection<ModEntry> Mods { get; set; }
        public string Name { get; set; }
    }

    public class ModEntry
    {
        public bool Default { get; set; }
        public Mod Mod { get; set; }

        public bool Required { get; set; }

    }

    public class Mod
    {
        public IDictionary<Uri, FileSignature> Files { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }
    }
}
