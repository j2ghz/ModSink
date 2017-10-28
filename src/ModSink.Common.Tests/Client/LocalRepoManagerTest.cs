using ModSink.Common;
using ModSink.Common.Client;
using ModSink.Core.Client;
using ModSink.Core.Models.Repo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using Xunit;
using System.Threading.Tasks;

namespace Modsink.Common.Tests.Client
{
    public class LocalRepoManagerTest
    {
        private readonly ILocalStorageService manager = new LocalStorageService(new Uri(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())));

        [Fact]
        public async Task WriteReadAndDelete()
        {
            var hash = new XXHash64().HashOfEmpty;
            (await this.manager.IsFileAvailable(hash)).Should().Be(false);
            await Assert.ThrowsAsync<FileNotFoundException>(async () => await this.manager.Read(hash));
            using (var stream = await this.manager.Write(hash))
            {
                stream.WriteByte(0xff);
            }
            (await this.manager.IsFileAvailable(hash)).Should().Be(true);
            using (var stream = await this.manager.Read(hash))
            {
                stream.ReadByte().Should().Be(0xff);
                stream.ReadByte().Should().Be(-1);
            }
            await this.manager.Delete(hash);
            (await this.manager.IsFileAvailable(hash)).Should().Be(false);
            await Assert.ThrowsAsync<FileNotFoundException>(async () => await this.manager.Read(hash));
        }
    }
}