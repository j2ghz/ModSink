using System;

namespace ModSink.Common.Models.Repo
{
    [Serializable]
    public class ModEntry
    {
        public bool Default { get; set; }
        public Mod Mod { get; set; }

        public bool Required { get; set; }
    }
}