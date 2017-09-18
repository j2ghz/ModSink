using System;
using System.Collections.Generic;
using System.Text;

namespace ModSink.Core.Models.Repo
{
    [Serializable]
    public class ModEntry
    {
        public bool Default { get; set; }
        public Mod Mod { get; set; }

        public bool Required { get; set; }
    }
}