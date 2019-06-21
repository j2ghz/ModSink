using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ModSink.Domain.Entities.File;

namespace ModSink.Application.Hashing
{
    public interface IHashFunction
    {
        Hash HashOfEmpty { get; }

        Task<Hash> ComputeHashAsync(Stream data, CancellationToken cancellationToken);
    }
}
