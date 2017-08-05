using System;
using System.IO;
using System.Threading;
using Xunit;

namespace ModSink.Common.Tests
{
    public class BuilderTests
    {
        [Fact]
        public async System.Threading.Tasks.Task DefaultBuildReturnsHashAsync()
        {
            var modsink = new Builder().Build();
            Assert.NotNull(modsink);
            Assert.NotNull(modsink.HashFunction);
            ModSink.Core.Models.Local.IHashValue hash = await modsink.HashFunction.ComputeHashAsync(new MemoryStream(new byte[] { 0, 1 }), CancellationToken.None);
        }
    }
}