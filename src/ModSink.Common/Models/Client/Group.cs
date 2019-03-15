using System;
using System.Collections.Generic;
using System.Text;

namespace ModSink.Common.Models.Client
{
    public class Group
    {
        public ICollection<Repo> Repos { get; set; }
        public string Name { get; set; }
    }
}
