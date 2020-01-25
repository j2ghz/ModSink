using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ModSink.Application.Hashing;
using ModSink.Domain.Entities.File;

namespace ModSink.Infrastructure.Hashing
{
    public class XXHash64 : IHashFunction
    {
        public Hash HashOfEmpty => throw new NotImplementedException();
        public int HashSize { get; } = 64;
        public Hash CreateHash(byte[] rawBytes)
        {
            return new Hash("XXHash64", rawBytes);
        }

        public Hash ComputeHash(Stream data, CancellationToken cancellationToken)
        {
            var bytes = xxHash64.Create().ComputeHash(data);
            return CreateHash(bytes);
        }

        public Task<Hash> ComputeHashAsync(Stream data, CancellationToken cancellationToken)
        {
            return Task.Run(() => ComputeHash(data, cancellationToken), cancellationToken);
        }

        public HashAlgorithm AsHashAlgorithm()
        {
            return xxHash64.Create();
        }


    }
}
