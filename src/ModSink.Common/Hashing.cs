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
            using (var stream = file.OpenRead())
            {
                return await this.hashFunction.ComputeHashAsync(stream, cancel);
            }
        }

        public IObservable<FileWithHash> GetFileHashes(DirectoryInfo directory)
        {
            return GetFiles(directory)
                .Select(fi => Observable.DeferAsync(async token => Observable.Return(new FileWithHash(fi, await GetFileHash(fi, token))))).Switch();
        }

        public IObservable<FileInfo> GetFiles(DirectoryInfo directory)
        {
            return Observable.Create((IObserver<FileInfo> observer) =>
            {
                var directoryStack = new Stack<DirectoryInfo>();
                directoryStack.Push(directory);

                while (directoryStack.Count > 0)
                {
                    var dir = directoryStack.Pop();

                    dir.EnumerateDirectories().ForEach(directoryStack.Push);

                    dir.EnumerateFiles().ForEach(observer.OnNext);
                }

                observer.OnCompleted();
                return Disposable.Empty;
            });
        }
    }
}