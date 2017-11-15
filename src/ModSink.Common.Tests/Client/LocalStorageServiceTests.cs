using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using ModSink.Common;
using ModSink.Common.Client;
using ModSink.Core.Client;
using ModSink.Core.Models.Repo;
using Xunit;

namespace Modsink.Common.Tests.Client
{
    public class LocalStorageServiceTests
    {
        private readonly ILocalStorageService manager =
            new LocalStorageService(new Uri(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())));

        [Fact]
        public async Task CheckLocationTest()
        {
            var uri = new Uri(Path.Combine(Path.GetFullPath("."), "temp\\"));
            var lss = new LocalStorageService(uri);
            var hash = new HashValue(new byte[] {0x99, 0xE9, 0xD8, 0x51, 0x37, 0xDB, 0x46, 0xEF});

            if (await lss.IsFileAvailable(hash))
                await lss.Delete(hash);
            (await lss.IsFileAvailable(hash)).Should().BeFalse();
            (await lss.Write(hash)).Close();
            (await lss.IsFileAvailable(hash)).Should().BeTrue();
            var filename = (await lss.GetFileInfo(hash)).FullName;
            filename.Should().StartWith(Path.Combine(Path.GetFullPath("."), "temp\\"));
            filename.Should().ContainEquivalentOf(hash.ToString());
            await lss.Delete(hash);
            (await lss.IsFileAvailable(hash)).Should().BeFalse();
        }

        [Fact]
        public async Task WriteReadAndDelete()
        {
            var hash = new XXHash64().HashOfEmpty;
            (await manager.IsFileAvailable(hash)).Should().Be(false);
            await Assert.ThrowsAsync<FileNotFoundException>(async () => await manager.Read(hash));
            using (var stream = await manager.Write(hash))
            {
                stream.WriteByte(0xff);
            }
            (await manager.IsFileAvailable(hash)).Should().Be(true);
            using (var stream = await manager.Read(hash))
            {
                stream.ReadByte().Should().Be(0xff);
                stream.ReadByte().Should().Be(-1);
            }
            await manager.Delete(hash);
            (await manager.IsFileAvailable(hash)).Should().Be(false);
            await Assert.ThrowsAsync<FileNotFoundException>(async () => await manager.Read(hash));
        }
    }
}