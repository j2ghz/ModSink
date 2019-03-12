using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ModSink.Common.Models.DTO.Repo;

namespace ModSink.Common
{
    public interface IHashFunction
    {
        HashValue HashOfEmpty { get; }

        Task<HashValue> ComputeHashAsync(Stream data, CancellationToken cancellationToken);
    }
}