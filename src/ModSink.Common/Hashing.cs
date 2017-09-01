using ModSink.Core;
using System;
using System.Collections.Generic;
using System.Text;
using ModSink.Core.Models.Repo;
using System.IO;
using System.Threading;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Linq;

namespace ModSink.Common
{
    public class Hashing : IHashing
    {
        private readonly IHashFunction hashFunction;

        public Hashing(IHashFunction hashFunction)
        {
            this.hashFunction = hashFunction;
        }

        public async Task<HashValue> GetFileHash(FileInfo file, CancellationToken cancel)
        {
            using (var stream = file.OpenRead())
            {
                return await this.hashFunction.ComputeHashAsync(stream, cancel);
            }
        }

        public IObservable<(HashValue hash, FileInfo file)> GetFileHashes(DirectoryInfo directory)
        {
            return GetFileHashes(directory.EnumerateFiles("*", SearchOption.AllDirectories));
        }

        public IObservable<(HashValue hash, FileInfo file)> GetFileHashes(IEnumerable<FileInfo> files)
        {
            return files
                .ToObservable()
                .SelectMany(f => Observable.FromAsync(cancel => GetFileHash(f, cancel))
                .Select(res => (res, f)));
        }
    }
}