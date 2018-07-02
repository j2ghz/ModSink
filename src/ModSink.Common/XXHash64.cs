using System.Data.HashFunction.xxHash;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ModSink.Common.Models.Repo;

namespace ModSink.Common
{
    public class XXHash64 : IHashFunction
    {
        private readonly IxxHash xx;

        public XXHash64()
        {
            xx = xxHashFactory.Instance.Create(new xxHashConfig {HashSizeInBits = 64});
        }

        public async Task<HashValue> ComputeHashAsync(Stream data, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var hash = await xx.ComputeHashAsync(data, cancellationToken);
            return new HashValue(hash.Hash);
        }

        public HashValue HashOfEmpty => new HashValue(new byte[] {0x99, 0xE9, 0xD8, 0x51, 0x37, 0xDB, 0x46, 0xEF});
    }
}