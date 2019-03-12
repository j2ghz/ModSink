using System;
using System.Collections.Generic;

namespace ModSink.Common.Models.DTO.Repo
{
    [Serializable]
    public class Repo : WithBaseUri
    {
        public IDictionary<FileSignature, Uri> Files { get; set; }

        public ICollection<Modpack> Modpacks { get; set; }
    }
}