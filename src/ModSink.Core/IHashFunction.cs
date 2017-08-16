using ModSink.Core.Models.Repo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ModSink.Core
{
    public interface IHashFunction
    {
        Task<HashValue> ComputeHashAsync(Stream data, CancellationToken cancellationToken);
    }
}