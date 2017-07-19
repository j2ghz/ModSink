using ModSink.Core.Models.Local;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ModSink.Common
{
    public class XXHash64 : System.Data.HashFunction.xxHash, IHashFunction<XXHash64Value>
    {
        async Task<XXHash64Value> IHashFunction<XXHash64Value>.ComputeHashAsync(Stream data, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var hash = await base.ComputeHashAsync(data);
            return new XXHash64Value(hash);
        }
    }
}