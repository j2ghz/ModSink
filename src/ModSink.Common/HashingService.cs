using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModSink.Common.Models.DTO.Repo;

namespace ModSink.Common
{
    public class HashingService
    {
        private readonly IHashFunction hashFunction;

        public HashingService(IHashFunction hashFunction)
        {
            this.hashFunction = hashFunction;
        }

        public async Task<HashValue> GetFileHash(FileInfo file, CancellationToken cancel)
        {
            if (file.Length > 0)
                using (var stream = file.OpenRead())
                {
                    return await hashFunction.ComputeHashAsync(stream, cancel);
                }

            return hashFunction.HashOfEmpty;
        }

        public async Task<HashValue> GetFileHash(Stream stream, CancellationToken cancel)
        {
            return await hashFunction.ComputeHashAsync(stream, cancel);
        }

        public IEnumerable<Lazy<Task<FileWithHash>>> GetFileHashes(DirectoryInfo directory, CancellationToken token)
        {
            foreach (var file in GetFiles(directory))
            {
                token.ThrowIfCancellationRequested();
                yield return new Lazy<Task<FileWithHash>>(async () =>
                {
                    var hash = await GetFileHash(file, token);
                    return new FileWithHash(file, hash);
                });
            }
        }

        public IEnumerable<FileInfo> GetFiles(DirectoryInfo directory)
        {
            var directoryStack = new Stack<DirectoryInfo>();
            directoryStack.Push(directory);

            while (directoryStack.Count > 0)
            {
                var dir = directoryStack.Pop();
                foreach (var d in dir.EnumerateDirectories())
                {
                    directoryStack.Push(d);
                }
                foreach (var file in dir.EnumerateFiles()) yield return file;
            }
        }
    }
}