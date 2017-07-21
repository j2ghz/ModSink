using ModSink.Core.Models.Local;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ModSink.Common
{
    public class XXHash64 : System.Data.HashFunction.xxHash, IHashFunction<ByteHashValue>
    {
        async Task<ByteHashValue> IHashFunction<ByteHashValue>.ComputeHashAsync(Stream data, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var hash = await base.ComputeHashAsync(data);
            return new ByteHashValue(hash);
        }
    }
}