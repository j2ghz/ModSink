using ModSink.Common;
using ModSink.Core.Models.Repo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Xunit;

namespace Modsink.Common.Tests
{
    public class XXHash64Test
    {
        [Theory]
        [InlineData(new byte[] { 0x99, 0xE9, 0xD8, 0x51, 0x37, 0xDB, 0x46, 0xEF }, new byte[0])]
        [InlineData(new byte[] { 0x51, 0x0E, 0x4B, 0xA5, 0x94, 0x04, 0x76, 0x90 }, new byte[] { 0x99, 0xE9, 0xD8, 0x51, 0x37, 0xDB, 0x46, 0xEF })]
        public async System.Threading.Tasks.Task ComputeHashAsync(byte[] expected, byte[] data)
        {
            var expectedV = new HashValue(expected);

            var stream = new MemoryStream(data);

            var hash = new XXHash64();

            var result = await hash.ComputeHashAsync(stream, CancellationToken.None);

            Assert.Equal(expectedV, result);

            Assert.Equal<byte>(expectedV.Value, result.Value);
        }
    }
}