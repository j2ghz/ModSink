using System.Data.HashFunction.xxHash;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ModSink.Application.Hashing;
using ModSink.Domain.Entities.File;

namespace ModSink.Infrastructure.Hashing
{
    public class XXHash64 : IHashFunction
    {
        private readonly IxxHash xxHash;

        public XXHash64()
        {
            xxHash = xxHashFactory.Instance.Create(new xxHashConfig {HashSizeInBits = 64});
        }

        public Hash ComputeHash(Stream data, CancellationToken cancellationToken)
        {
            var hash = xxHash.ComputeHash(data, cancellationToken);
            return new XXHashHash(hash.Hash);
        }

        public async Task<Hash> ComputeHashAsync(Stream data, CancellationToken cancellationToken)
        {
            var hashValue = await xxHash.ComputeHashAsync(data, cancellationToken);
            return new XXHashHash(hashValue.Hash);
        }

        public Hash HashOfEmpty { get; }
        public int HashSize => 64;

        private class XXHashHash : Hash
        {
            public XXHashHash(byte[] value) : base(value)
            {
            }

            public override string HashId => "XXHash64";
        }
    }
}