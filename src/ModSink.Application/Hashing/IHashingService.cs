using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using ModSink.Domain.Entities.File;
using ModSink.Domain.Entities.Repo;

namespace ModSink.Application.Hashing
{
    public interface IHashingService
    {
        IEnumerable<Task<(RelativePathFile File, List<Chunk> Chunks)>> GetFileHashes(IDirectoryInfo directory,
            CancellationToken token);
    }
}
