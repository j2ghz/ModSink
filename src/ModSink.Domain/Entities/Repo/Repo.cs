using System.Collections.Generic;
using ModSink.Domain.Entities.File;
using PathLib;

namespace ModSink.Domain.Entities.Repo
{
    public class Repo
    {
        public string Name { get; set; }
        public IReadOnlyDictionary<FileSignature, IPurePath> SourceFiles { get; set; }
        public IReadOnlyCollection<FileChunk> FileChunks { get; set; }
        public ICollection<Modpack> Modpacks { get; set; }
    }
}