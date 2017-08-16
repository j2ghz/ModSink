using ModSink.Core.Models.Repo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModSink.Core
{
    public interface IHashing
    {
        Task<HashValue> GetFileHash(FileInfo file, CancellationToken cancellationToken);

        IObservable<(HashValue hash, FileInfo file)> GetFileHashes(DirectoryInfo directory);

        IObservable<(HashValue hash, FileInfo file)> GetFileHashes(IEnumerable<FileInfo> files);
    }
}