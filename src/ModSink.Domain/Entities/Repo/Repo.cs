using System;
using System.Collections.Generic;

namespace ModSink.Domain.Entities.Repo
{
    public class Repo
    {
        public string Name { get; private set; }
        public ICollection<RelativeUriFile> Files { get; private set; }
        public ICollection<Modpack> Modpacks { get; private set; }
    }
}