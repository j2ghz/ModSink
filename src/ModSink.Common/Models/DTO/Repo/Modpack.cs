using System;
using System.Collections.Generic;

namespace ModSink.Common.Models.DTO.Repo
{
    [Serializable]
    public class Modpack
    {
        public ICollection<ModEntry> Mods { get; set; }
        public string Name { get; set; }
    }
}