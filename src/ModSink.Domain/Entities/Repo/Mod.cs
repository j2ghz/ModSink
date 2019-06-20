using System;
using System.Collections.Generic;
using System.Text;

namespace ModSink.Domain.Entities.Repo
{
    public class Mod
    {
        public string Name { get;  set; }
        public ICollection<RelativeUriFile> Files { get;  set; }
    }
}
