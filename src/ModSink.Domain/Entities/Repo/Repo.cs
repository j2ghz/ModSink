using System;
using System.Collections.Generic;

namespace ModSink.Domain.Entities.Repo
{
    public class Repo
    {
        public string Name { get; set; }
        public ICollection<RelativeUriFile> Files { get;  set; }
        public ICollection<Modpack> Modpacks { get;  set; }
    }
}