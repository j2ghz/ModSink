using Microsoft.Reactive.Testing;
using ModSink.Common;
using ModSink.Core.Models.Repo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
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

            var obs = hashing.GetFileHashes(files);
            obs.Subscribe(Observer.Create<(HashValue, FileInfo)>(t => Assert.Equal(hashF.HashOfEmpty, t.Item1)));
            obs.Wait();
            var result = obs.ToEnumerable();
            Assert.Equal(3, result.Count());
            foreach (var pair in result)
            {
                Assert.Equal(hashF.HashOfEmpty, pair.hash);
                Assert.Equal(0, pair.file.Length);
            }
        }
    }
}