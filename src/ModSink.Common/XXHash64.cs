using ModSink.Core.Local;
using ModSink.Core.Models;
using ModSink.Core.Models.Repo;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ModSink.Common
{
    public class XXHash64 : System.Data.HashFunction.xxHash, IHashFunction
    {
        async Task<HashValue> IHashFunction.ComputeHashAsync(Stream data, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var hash = await base.ComputeHashAsync(data);
            return new HashValue(hash);
        }
    }
}