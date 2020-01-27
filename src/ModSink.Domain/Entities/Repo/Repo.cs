using System.Collections.Generic;
using ModSink.Domain.Entities.File;
using PathLib;

namespace ModSink.Domain.Entities.Repo {
  public class Repo {
    public Repo(string name, IReadOnlyCollection<Modpack>modpacks,
                string chunksPath) {
      Name = name;
      Modpacks = modpacks;
      ChunksPath = chunksPath;
    }

    public IReadOnlyCollection<Modpack>Modpacks { get; }
    public string Name { get; }
    public string ChunksPath { get; }
  }
}
