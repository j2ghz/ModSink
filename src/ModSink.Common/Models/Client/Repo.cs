using System;
using System.Collections.Generic;
using System.Text;
using ModSink.Common.Models.DTO.Repo;

namespace ModSink.Common.Models.Client
{
    public class Repo
    {
        public IReadOnlyDictionary<FileSignature, Uri> Files { get; set; }

        public IReadOnlyCollection<Modpack> Modpacks { get; set; }

        public Uri BaseUri { get; set; }

    }
}
