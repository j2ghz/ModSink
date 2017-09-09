using Microsoft.Reactive.Testing;
using ModSink.Common;
using ModSink.Core;
using ModSink.Core.Models.Repo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using Xunit;

namespace Modsink.Common.Tests
{
    public class HashingTest
    {
        [Fact]
        public void GetFileHashesEmpty()
        {
            var hashF = new XXHash64();
            var hashing = new Hashing(hashF);
            var files = Enumerable.Range(0, 3).Select(_ => Path.GetTempFileName()).Select(path => new FileInfo(path));

            var obs = files.ToObservable().SelectMany(async fi => new FileWithHash(fi, await hashing.GetFileHash(fi, CancellationToken.None)));
            obs.Subscribe(Observer.Create<FileWithHash>(t => Assert.Equal(hashF.HashOfEmpty, t.Hash)));
            obs.Wait();
            var result = obs.ToEnumerable();
            Assert.Equal(3, result.Count());
            foreach (var pair in result)
            {
                Assert.Equal(hashF.HashOfEmpty, pair.Hash);
                Assert.Equal(0, pair.File.Length);
            }
        }
    }
}