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
using System.Reactive.Disposables;

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
            if (file.Length > 0)
            {
                using (var stream = file.OpenRead())
                {
                    return await this.hashFunction.ComputeHashAsync(stream, cancel);
                }
            }
            else
            {
                return this.hashFunction.HashOfEmpty;
            }
        }

        public async Task<HashValue> GetFileHash(Stream stream, CancellationToken cancel)
        {
            return await this.hashFunction.ComputeHashAsync(stream, cancel);
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
                dir.EnumerateDirectories().ForEach(directoryStack.Push);
                foreach (var file in dir.EnumerateFiles())
                {
                    yield return file;
                }
            }
        }
    }
}