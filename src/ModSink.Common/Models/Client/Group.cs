using System.Collections.Generic;

namespace ModSink.Common.Models.Client
{
    public class Group
    {
        public string Name { get; set; }
        public ICollection<Repo> Repos { get; set; }
    }
}