using ModSink.Core.Models.Repo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModSink.Core
{
    public interface IHashingService
    {
        Task<HashValue> GetFileHash(FileInfo file, CancellationToken cancel);

        IEnumerable<Lazy<Task<FileWithHash>>> GetFileHashes(DirectoryInfo directory, CancellationToken token);

        IEnumerable<FileInfo> GetFiles(DirectoryInfo directory);
    }
}