using System.Collections.Generic;
using ModSink.Domain.Entities.File;

namespace ModSink.Application.Repo.Builder {
  public class RepoWithFileChunks {
    public RepoWithFileChunks(Domain.Entities.Repo.Repo repo,
                              IReadOnlyCollection<FileChunks>fileChunks) {
      Repo = repo;
      FileChunks = fileChunks;
    }

    public Domain.Entities.Repo.Repo Repo { get; }
    public IReadOnlyCollection<FileChunks>FileChunks { get; }
  }
}
