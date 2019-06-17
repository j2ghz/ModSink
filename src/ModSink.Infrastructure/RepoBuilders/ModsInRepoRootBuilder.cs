using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text;
using ModSink.Application;
using ModSink.Domain.Entities.Repo;

namespace ModSink.Infrastructure.RepoBuilders
{
    public class ModsInRepoRootBuilder : IRepoBuilder
    {
        public Repo Build(IDirectoryInfo root)
        {
            throw new NotImplementedException();
        }
    }
}
