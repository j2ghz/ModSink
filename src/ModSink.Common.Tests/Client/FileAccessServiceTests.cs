using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using ModSink.Common.Client;
using ModSink.Common.Models.Repo;
using Xunit;

namespace ModSink.Common.Tests.Client
{
    public class FileAccessServiceTests
    {
        [Fact]
        public async Task CheckLocationTest()
        {
            var lss = new FileAccessService(new DirectoryInfo(Path.Combine(Path.GetFullPath("."), "temp\\")));
            var fileSignature =
                new FileSignature(new HashValue(new byte[] {0x99, 0xE9, 0xD8, 0x51, 0x37, 0xDB, 0x46, 0xEF}), 1UL);

            if (await lss.IsFileAvailable(fileSignature))
                await lss.Delete(fileSignature);
            (await lss.IsFileAvailable(fileSignature)).Should().BeFalse();
            using (var stream = await lss.Write(fileSignature))
            {
                stream.WriteByte(0xff);
            }

            (await lss.IsFileAvailable(fileSignature)).Should().BeTrue();
            using (var stream = await lss.Read(fileSignature))
            {
                stream.ReadByte().Should().Be(0xff);
                stream.ReadByte().Should().Be(-1);
            }

            var filename = (await lss.GetFileInfo(fileSignature)).FullName;
            filename.Should().StartWith(Path.Combine(Path.GetFullPath("."), "temp\\"));
            filename.Should().ContainEquivalentOf(fileSignature.Hash.ToString());
            await lss.Delete(fileSignature);
            (await lss.IsFileAvailable(fileSignature)).Should().BeFalse();
            await Assert.ThrowsAsync<FileNotFoundException>(async () => await lss.Read(fileSignature));
        }
    }
}