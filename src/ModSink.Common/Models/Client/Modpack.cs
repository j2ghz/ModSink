using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModSink.Common.Models.DTO.Repo;

namespace ModSink.Common.Models.Client
{
    public class Modpack
    {
        public Modpack(DTO.Repo.Modpack modpack)
        {
            Name = modpack.Name;
            Mods = modpack.Mods.Select(me => new ModEntry(me)).ToList();
        }

        public Guid Id { get; set; } = Guid.NewGuid();
        public ICollection<ModEntry> Mods { get; set; }
        public string Name { get; set; }
    }

    public class ModEntry
    {

        public ModEntry(DTO.Repo.ModEntry me)
        {
            Default = me.Default;
            Required = me.Required;
            Mod = new Mod(me.Mod);
        }

        public bool Default { get; set; }
        public Mod Mod { get; set; }

        public bool Required { get; set; }

    }

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
