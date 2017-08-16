using ModSink.Core;
using ModSink.Core.Models.Repo;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace ModSink.Common
{
    public class XXHash64 : System.Data.HashFunction.xxHash, IHashFunction
    {
        public XXHash64() : base(64)
        {
        }

        public HashValue HashOfEmpty => new HashValue(new byte[] { 0x99, 0xE9, 0xD8, 0x51, 0x37, 0xDB, 0x46, 0xEF });

        public async Task<HashValue> ComputeHashAsync(Stream data, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var hash = await base.ComputeHashAsync(data);
            return new HashValue(hash);
        }
    }
}