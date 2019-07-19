using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using ModSink.Application.RepoBuilders;
using ModSink.Domain.Entities.Repo;

namespace ModSink.Application
{
    public interface IRepoBuilder
    {
        Task<RepoWithFileChunks> Build(IDirectoryInfo root, CancellationToken token);
    }

    public interface IRepoBuilder<TConfig> : IRepoBuilder
    {
        Task<RepoWithFileChunks> Build(IDirectoryInfo root, TConfig config, CancellationToken token);
    }
}