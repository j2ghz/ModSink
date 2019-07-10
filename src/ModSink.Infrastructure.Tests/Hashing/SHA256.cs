using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ModSink.Application.Hashing;
using ModSink.Domain.Entities.File;

namespace ModSink.Infrastructure.Tests.Hashing
{
    public class SHA256 : IHashFunction
    {
        public Hash ComputeHash(Stream data, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Hash> ComputeHashAsync(Stream data, CancellationToken cancellationToken)
        {
            var bytes = System.Security.Cryptography.SHA256.Create().ComputeHash(data);
            return Task.FromResult((Hash) new SHA256Hash(bytes));
        }

        public Hash HashOfEmpty { get; }
        public int HashSize { get; } = 256;

        public class SHA256Hash : Hash
        {
            public SHA256Hash(byte[] bytes) : base(bytes)
            {
            }

            public override string HashId { get; } = "SHA256";
        }
    }
}