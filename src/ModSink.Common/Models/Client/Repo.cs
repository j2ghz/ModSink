using System;
using System.Collections.Generic;
using ModSink.Common.Models.DTO.Repo;

namespace ModSink.Common.Models.Client
{
    public class Repo
    {
        public Uri BaseUri { get; set; }
        public IReadOnlyDictionary<FileSignature, Uri> Files { get; set; }
        public IReadOnlyCollection<Modpack> Modpacks { get; set; }
    }
}