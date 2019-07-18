using System.Collections.Generic;
using ModSink.Domain.Entities.File;
using PathLib;

namespace ModSink.Domain.Entities.Repo
{
    [Equals]
    public class Repo
    {
        public Repo(string name, ICollection<Modpack> modpacks, IReadOnlyDictionary<FileSignature, IPurePath> sourceFiles, IReadOnlyCollection<FileChunk> fileChunks)
        {
            Name = name;
            Modpacks = modpacks;
            SourceFiles = sourceFiles;
            FileChunks = fileChunks;
        }

        public IReadOnlyCollection<FileChunk> FileChunks { get; }
        public ICollection<Modpack> Modpacks { get; }
        public string Name { get; }
        public IReadOnlyDictionary<FileSignature, IPurePath> SourceFiles { get; }
    }
}