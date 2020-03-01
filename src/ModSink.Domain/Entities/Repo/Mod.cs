using System.Collections.Generic;

namespace ModSink.Domain.Entities.Repo
{
    public class Mod
    {
        public ICollection<RelativePathFile> Files { get; set; }
        public string Name { get; set; }
    }
}
