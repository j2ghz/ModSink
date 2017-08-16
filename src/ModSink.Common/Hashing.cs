using ModSink.Core;
using System;
using System.Collections.Generic;
using System.Text;
using ModSink.Core.Models.Repo;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace ModSink.Common
{
    public class Hashing : IHashing
    {
        private readonly IHashFunction hashFunction;

        public Hashing(IHashFunction hashFunction)
        {
            this.hashFunction = hashFunction;
        }

        public async System.Threading.Tasks.Task<HashValue> GetFileHash(FileInfo file, CancellationToken cancel)
        {
            Console.WriteLine("Opening " + file.FullName);
            using (var fileStream = file.OpenRead())
            {
                using (var mmfile = MemoryMappedFile.CreateFromFile(file.FullName))
                {
                    using (var mmfstream = mmfile.CreateViewStream())
                    {
                        return await this.hashFunction.ComputeHashAsync(mmfstream, cancel);
                    }
                }
            }
        }

        public IObservable<(HashValue, FileInfo)> GetFileHashes(DirectoryInfo directory)
        {
            var obs = GetFileHashes(directory.EnumerateFiles());
            foreach (var dir in directory.EnumerateDirectories())
            {
                obs.Merge(GetFileHashes(dir));
            }
            return obs;
        }

        public IObservable<(HashValue, FileInfo)> GetFileHashes(IEnumerable<FileInfo> files)
        {
            var obs = files.ToObservable();
            return obs.SelectMany(f => Observable.FromAsync(cancel => GetFileHash(f, cancel)).Select(res => (res, f)));
        }
    }
}