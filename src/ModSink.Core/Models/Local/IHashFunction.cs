using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ModSink.Core.Models.Local
{
    public interface IHashFunction
    {
        Task<IHashValue> ComputeHashAsync(Stream data, CancellationToken cancellationToken);
    }
}