using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ModSink.Domain.Entities.Repo;

namespace ModSink.Application
{
    public interface IRepoBuilder
    {
        Task<Repo> Build(System.IO.Abstractions.IDirectoryInfo root, CancellationToken token);
    }

    public interface IRepoBuilder<TConfig> : IRepoBuilder
    {
        Task<Repo> Build(System.IO.Abstractions.IDirectoryInfo root, TConfig config, CancellationToken token);
    }
}
