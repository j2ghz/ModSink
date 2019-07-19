using System.Collections.Generic;
using ModSink.Domain.Entities.File;
using ModSink.Domain.Entities.Repo;

namespace ModSink.Application.RepoBuilders
{
    public class RepoWithFileChunks
    {
        public RepoWithFileChunks(Repo repo, IReadOnlyCollection<FileChunks> fileChunks)
        {
            Repo = repo;
            FileChunks = fileChunks;
        }

        public Repo Repo { get; }
        public IReadOnlyCollection<FileChunks> FileChunks { get; }
    }
}