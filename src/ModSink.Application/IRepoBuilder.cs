using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using ModSink.Domain.Entities.Repo;

namespace ModSink.Application
{
    public interface IRepoBuilder
    {
        Task<Repo> Build(IDirectoryInfo root, CancellationToken token);
    }

    public interface IRepoBuilder<TConfig> : IRepoBuilder
    {
        Task<Repo> Build(IDirectoryInfo root, TConfig config, CancellationToken token);
    }
}