using System;
using System.Collections.Generic;
using System.Linq;

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
}