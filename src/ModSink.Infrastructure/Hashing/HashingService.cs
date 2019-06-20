using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ModSink.Application.Hashing;
using ModSink.Domain.Entities.File;
using ModSink.Domain.Entities.Repo;

namespace ModSink.Infrastructure.Hashing
{
    public class HashingService : IHashingService, IDisposable
    {
        private readonly IFileOpener _fileOpener;
        private readonly IHashFunction _hashFunction;
        private readonly SemaphoreSlim _semaphore;

        public HashingService(IHashFunction hashFunction, IOptions<Options> options, IFileOpener fileOpener)
        {
            _hashFunction = hashFunction;
            _fileOpener = fileOpener;
            _semaphore = new SemaphoreSlim(0, options.Value.Parallelism);
        }

        public void Dispose()
        {
            _semaphore?.Dispose();
        }

        public async IAsyncEnumerable<RelativeUriFile> GetFileHashes(IDirectoryInfo directory, CancellationToken token)
        {
            var baseUri = new Uri(directory.FullName);
            foreach (var file in GetFiles(directory))
            {
                token.ThrowIfCancellationRequested();
                var uri = new Uri(file.FullName);

                var hashTask = Task.Run(async () =>
                {
                    var fileHash = await GetFileHash(file, token);
                    return new RelativeUriFile
                    {
                        Signature = new FileSignature(fileHash, file.Length),
                        RelativeUri = RelativeUri.FromAbsolute(baseUri, uri)
                    };
                }, token);

                var hash = RunASyncWaitSemaphore(hashTask, _semaphore, token);

                yield return await hash;
            }
        }

        public async Task<Hash> GetFileHash(IFileInfo file, CancellationToken cancel)
        {
            if (file.Length <= 0) return _hashFunction.HashOfEmpty;
            await using var stream = _fileOpener.OpenRead(file);
            return await _hashFunction.ComputeHashAsync(stream, cancel);
        }

        public async Task<FileSignature> GetFileSignature(IFileInfo file, CancellationToken cancel)
        {
            return new FileSignature(await GetFileHash(file, cancel), file.Length);
        }

        public async Task<Hash> GetFileHash(Stream stream, CancellationToken cancel)
        {
            return await _hashFunction.ComputeHashAsync(stream, cancel);
        }

        public IEnumerable<IFileInfo> GetFiles(IDirectoryInfo directory)
        {
            var directoryStack = new Stack<IDirectoryInfo>();
            directoryStack.Push(directory);

            while (directoryStack.Count > 0)
            {
                var dir = directoryStack.Pop();
                foreach (var d in dir.EnumerateDirectories()) directoryStack.Push(d);
                foreach (var file in dir.EnumerateFiles()) yield return file;
            }
        }

        public async Task<T> RunASyncWaitSemaphore<T>(Task<T> task, SemaphoreSlim semaphoreSlim,
            CancellationToken token)
        {
            await semaphoreSlim.WaitAsync(token);
            T result;
            try
            {
                result = await task;
            }
            finally
            {
                semaphoreSlim.Release();
            }

            return result;
        }

        public class Options
        {
            public int Parallelism { get; set; }
        }
    }
}