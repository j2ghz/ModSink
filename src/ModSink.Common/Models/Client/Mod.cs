using System;
using System.Collections.Generic;
using ModSink.Common.Models.DTO.Repo;

namespace ModSink.Common.Models.Client
{
    public class Mod
    {
        public Mod(DTO.Repo.Mod mod)
        {
            Name = mod.Name;
            Version = mod.Version;
            Files = mod.Files;
        }

        public IDictionary<Uri, FileSignature> Files { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }
    }
}