using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using ModSink.Domain.Entities.File;
using ModSink.Domain.Entities.Repo;
using ModSink.Infrastructure.Hashing;
using ModSink.Infrastructure.RepoBuilders;
using ModSink.Infrastructure.Tests.Hashing;
using PathLib;
using Xunit;

namespace ModSink.Infrastructure.Tests.RepoBuilders
{
    public class ModsInRepoRootBuilderTests
    {
        [Fact]
        public async Task WithoutConfig()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"/repo/mod1/a.txt", new MockFileData("a")},
                {@"/repo/mod1/b.txt", new MockFileData("b")},
                {@"/repo/mod2/c.txt", new MockFileData("c")}
            });
            var hf = new SHA256();
            var hashingService = new HashingService(hf, Options.Create(new HashingService.Options()),
                new FileStreamOpener(
                    Options.Create(new FileStreamOpener.Options()),
                    new MockFileStreamFactory(fileSystem)));
            var builder = new ModsInRepoRootBuilder(hashingService);
            var root = fileSystem.DirectoryInfo.FromDirectoryName(@"/repo/");

            var repo = await builder.Build(root, CancellationToken.None);

            repo.Name.Should().Be("repo");
            repo.Files.Should().HaveCount(3);
            repo.Files.Should().Equal(new RelativePathFile
            {
                RelativePath = PurePath.Create("mod1\\a.txt"),
                Signature = new FileSignature(
                    new SHA256.SHA256Hash(new byte[]
                    {
                        0xCA, 0x97, 0x81, 0x12, 0xCA, 0x1B, 0xBD, 0xCA, 0xFA, 0xC2, 0x31, 0xB3, 0x9A, 0x23, 0xDC,
                        0x4D, 0xA7, 0x86, 0xEF, 0xF8, 0x14, 0x7C, 0x4E, 0x72, 0xB9, 0x80, 0x77, 0x85, 0xAF, 0xEE,
                        0x48, 0xBB
                    }),
                    1UL)
            }, new RelativePathFile
            {
                RelativePath = PurePath.Create("mod1\\b.txt"),
                Signature = new FileSignature(
                    new SHA256.SHA256Hash(new byte[]
                        {
                            0x3E, 0x23, 0xE8, 0x16, 0x00, 0x39, 0x59, 0x4A, 0x33, 0x89, 0x4F, 0x65, 0x64, 0xE1, 0xB1,
                            0x34, 0x8B, 0xBD, 0x7A, 0x00, 0x88, 0xD4, 0x2C, 0x4A, 0xCB, 0x73, 0xEE, 0xAE, 0xD5, 0x9C,
                            0x00, 0x9D
                        }
                    ),
                    1UL)
            }, new RelativePathFile
            {
                RelativePath = PurePath.Create("mod2\\c.txt"),
                Signature = new FileSignature(
                    new SHA256.SHA256Hash(new byte[]
                        {
                            0x2E, 0x7D, 0x2C, 0x03, 0xA9, 0x50, 0x7A, 0xE2, 0x65, 0xEC, 0xF5, 0xB5, 0x35, 0x68, 0x85,
                            0xA5, 0x33, 0x93, 0xA2, 0x02, 0x9D, 0x24, 0x13, 0x94, 0x99, 0x72, 0x65, 0xA1, 0xA2, 0x5A,
                            0xEF, 0xC6
                        }
                    ),
                    1UL)
            });
            repo.Modpacks.Should().HaveCount(1);
            repo.Modpacks.Single().Mods.Select(m => m.Name).Should().Contain(new[] {"mod1", "mod2"});
        }
    }
}