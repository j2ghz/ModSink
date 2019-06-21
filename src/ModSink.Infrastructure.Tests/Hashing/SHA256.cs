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
        public Hash HashOfEmpty { get; }
        public Task<Hash> ComputeHashAsync(Stream data, CancellationToken cancellationToken)
        {
            var bytes = System.Security.Cryptography.SHA256.Create().ComputeHash(data);
            return Task.FromResult(new Hash(bytes,"SHA256"));
        }
    }
}