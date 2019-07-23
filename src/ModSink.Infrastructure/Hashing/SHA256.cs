using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using ModSink.Application.Hashing;
using ModSink.Domain.Entities.File;

namespace ModSink.Infrastructure.Hashing
{
    public class SHA256 : IHashFunction
    {
        public Hash CreateHash(byte[] rawBytes)
        {
            return new SHA256Hash(rawBytes);
        }

        public Hash ComputeHash(Stream data, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var bytes = System.Security.Cryptography.SHA256.Create().ComputeHash(data);
            return new SHA256Hash(bytes);
        }

        public Task<Hash> ComputeHashAsync(Stream data, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var bytes = System.Security.Cryptography.SHA256.Create().ComputeHash(data);
            return Task.FromResult((Hash) new SHA256Hash(bytes));
        }

        public HashAlgorithm AsHashAlgorithm()
        {
            return System.Security.Cryptography.SHA256.Create();
        }

        public Hash HashOfEmpty => throw new NotImplementedException();
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