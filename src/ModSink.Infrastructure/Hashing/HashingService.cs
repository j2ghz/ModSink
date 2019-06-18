﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ModSink.Application.Hashing;
using ModSink.Domain.Entities.File;

namespace ModSink.Infrastructure.Hashing
{
    public class HashingService : IHashingService, IDisposable
    {
        private readonly IHashFunction _hashFunction;
        private readonly IFileOpener _fileOpener;
        private readonly SemaphoreSlim _semaphore;

        public HashingService(IHashFunction hashFunction, IOptions<Options> options, IFileOpener fileOpener)
        {
            this._hashFunction = hashFunction;
            _fileOpener = fileOpener;
            _semaphore = new SemaphoreSlim(0, options.Value.Parallelism);
        }

        public void Dispose()
        {
            _semaphore?.Dispose();
        }

        public IObservable<Hash> GetFileHashes(IDirectoryInfo directory)
        {
            return Observable.Create<Task<Hash>>(async (obs, token) =>
                {
                    foreach (var file in GetFiles(directory))
                    {
                        token.ThrowIfCancellationRequested();
                        var hash = RunASyncWaitSemaphore(GetFileHash(file, token), _semaphore, token);
                        obs.OnNext(hash);
                    }

                    obs.OnCompleted();
                }).SelectMany(x => x)
                .Replay()
                .RefCount();
        }

        public async Task<Hash> GetFileHash(IFileInfo file, CancellationToken cancel)
        {
            if (file.Length <= 0) return _hashFunction.HashOfEmpty;
            await using var stream = _fileOpener.OpenRead(file);
            return await _hashFunction.ComputeHashAsync(stream, cancel);
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