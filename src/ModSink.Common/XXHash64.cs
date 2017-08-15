using ModSink.Core.Models;
using ModSink.Core.Models.Local;
using System.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ModSink.Common
{
    [Export(typeof(IHashFunction))]
    public class XXHash64 : System.Data.HashFunction.xxHash, IHashFunction
    {
        public XXHash64() : base(64)
        {
        }

        async Task<IHashValue> IHashFunction.ComputeHashAsync(Stream data, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var hash = await base.ComputeHashAsync(data);
            return new ByteHashValue(hash);
        }
    }
}