using ModSink.Core.Models.Repo;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ModSink.Core.Local
{
    public interface IHashFunction
    {
        Task<HashValue> ComputeHashAsync(Stream data, CancellationToken cancellationToken);
    }
}