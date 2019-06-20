using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text;
using System.Threading;
using ModSink.Domain.Entities.File;
using ModSink.Domain.Entities.Repo;

namespace ModSink.Application.Hashing
{
    public interface IHashingService
    {
        IAsyncEnumerable<RelativeUriFile> GetFileHashes(IDirectoryInfo directory, CancellationToken token);
    }
}
