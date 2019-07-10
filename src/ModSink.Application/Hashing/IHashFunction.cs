using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ModSink.Domain.Entities.File;

namespace ModSink.Application.Hashing
{
    public interface IHashFunction
    {
        Hash HashOfEmpty { get; }
        int HashSize { get; }

        Hash ComputeHash(Stream data, CancellationToken cancellationToken);
        Task<Hash> ComputeHashAsync(Stream data, CancellationToken cancellationToken);
    }
}