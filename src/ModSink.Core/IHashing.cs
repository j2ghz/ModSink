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
        Task<HashValue> GetFileHash(FileInfo file, CancellationToken cancel);

        IObservable<FileWithHash> GetFileHashes(DirectoryInfo directory);

        IObservable<FileInfo> GetFiles(DirectoryInfo directory);
    }
}