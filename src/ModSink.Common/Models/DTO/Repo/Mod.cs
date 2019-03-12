using System;
using System.Collections.Generic;

namespace ModSink.Common.Models.DTO.Repo
{
    [Serializable]
    public class Mod
    {
        public IDictionary<Uri, FileSignature> Files { get; set; }
        public Guid Id { get; } = new Guid();

        public string Name { get; set; }

        public string Version { get; set; }
    }
}