using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ModSink.Application.Hashing;
using ModSink.Domain.Entities.File;
using ModSink.Domain.Entities.Repo;
using PathLib;

namespace ModSink.Infrastructure.Hashing
{
    public class HashingService : IHashingService, IDisposable
    {
        private readonly IFileOpener _fileOpener;
        private readonly IHashFunction _hashFunction;
        private readonly SemaphoreSlim _semaphore;
        private readonly StreamBreaker _streamBreaker;

        public HashingService(IHashFunction hashFunction, IOptions<Options> options, IFileOpener fileOpener,
            StreamBreaker streamBreaker)
        {
            _hashFunction = hashFunction;
            _fileOpener = fileOpener;
            _streamBreaker = streamBreaker;
            _semaphore = new SemaphoreSlim(options.Value.Parallelism);
        }

        public void Dispose()
        {
            _semaphore?.Dispose();
        }

        public IEnumerable<Task<(RelativePathFile File, List<Chunk> Chunks)>> GetFileHashes(IDirectoryInfo directory,
            CancellationToken token)
        {
            var root = PurePath.Create(directory.FullName);
            foreach (var file in GetFiles(directory))
            {
                token.ThrowIfCancellationRequested();
                var filePath = PurePath.Create(file.FullName);

                var hash = RunASyncWaitSemaphore(async () =>
                {
                    var (fileSignature, fileChunks) = await GetFileHash(file, token);
                    return (new RelativePathFile
                    {
                        Signature = fileSignature,
                        RelativePath = filePath.RelativeTo(root)
                    }, fileChunks);
                }, _semaphore, token);

                yield return hash;
            }
        }

        public async Task<(Signature fileSignature, List<Chunk> fileChunks)> GetFileHash(IFileInfo file,
            CancellationToken cancel)
        {
            using var stream = _fileOpener.OpenRead(file);
            var chunks = _streamBreaker.GetSegments(stream, file.Length).ToList();
            var chunkHashesStream =
                new MemoryStream(CombineByteArrays(chunks.Select(c => c.Hash.RawForHashing()).ToArray()));
            var fileHash = await _hashFunction.ComputeHashAsync(chunkHashesStream, cancel);
            var fileSig = new Signature(fileHash, file.Length);
            var fileChunks = chunks.Select(c => new Chunk
            { Signature = new Signature(c.Hash, c.Length), Position = c.Offset }).ToList();
            return (fileSig, fileChunks);
        }

        private byte[] CombineByteArrays(params byte[][] arrays)
        {
            var result = new byte[arrays.Sum(a => a.Length)];
            var i = 0;
            foreach (var array in arrays)
            {
                Buffer.BlockCopy(array, 0, result, i, array.Length);
                i += array.Length;
            }

            return result;
        }

        public async Task<Hash> GetFileHash(Stream stream, CancellationToken cancel)
        {
            return await _hashFunction.ComputeHashAsync(stream, cancel);
        }

        private static IEnumerable<IFileInfo> GetFiles(IDirectoryInfo directory)
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

        private static async Task<T> RunASyncWaitSemaphore<T>(Func<Task<T>> action, SemaphoreSlim semaphoreSlim,
            CancellationToken token)
        {
            await semaphoreSlim.WaitAsync(token);
            T result;
            try
            {
                result = await action();
            }
            finally
            {
                semaphoreSlim.Release();
            }

            return result;
        }

        public class Options
        {
            public int Parallelism { get; set; } = 2;
        }
    }
}