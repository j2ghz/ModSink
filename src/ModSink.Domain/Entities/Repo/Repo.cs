using System.Collections.Generic;
using ModSink.Domain.Entities.File;
using PathLib;

namespace ModSink.Domain.Entities.Repo
{
    public class Repo
    {
        public Repo(string name, IReadOnlyCollection<Modpack> modpacks, IReadOnlyDictionary<FileSignature, IPurePath> sourceFiles)
        {
            Name = name;
            Modpacks = modpacks;
            SourceFiles = sourceFiles;
        }

        public IReadOnlyCollection<Modpack> Modpacks { get; }
        public string Name { get; }
        public IReadOnlyDictionary<FileSignature, IPurePath> SourceFiles { get; }
    }
}