using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace ModSink.Application.Repo.Builder
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
