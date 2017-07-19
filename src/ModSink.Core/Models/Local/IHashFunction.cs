using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModSink.Core.Models.Local
{
    public interface IHashFunction<T>
        where T : IHashValue
    {
        Task<T> ComputeHashAsync(Stream data, CancellationToken cancellationToken);
    }
}