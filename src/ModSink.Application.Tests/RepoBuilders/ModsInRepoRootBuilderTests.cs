using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using ModSink.Application.Hashing;
using ModSink.Application.Repo.Builder;
using ModSink.Domain.Entities.File;
using ModSink.Domain.Entities.Repo;
using ModSink.Infrastructure.Hashing;
using PathLib;
using Xunit;

namespace ModSink.Application.Tests.RepoBuilders
{
public class ModsInRepoRootBuilderTests
{
    [Fact(Skip = "Use chunks")]
    public async Task WithoutConfig()
    {
        //Prepare env
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
                                                        new MockFileStreamFactory(fileSystem)), new StreamBreaker(hf));
        var builder = new ModsInRepoRootBuilder(hashingService);
        var root = fileSystem.DirectoryInfo.FromDirectoryName(@"/repo/");
        //Build
        var actual = await builder.Build(root, CancellationToken.None);
        //Manual repo to compare
        var fileA = new Signature(
            new Hash("SHA256", new byte[]
        {
            0x16, 0xD2, 0x1A, 0xE1, 0xF9, 0xBF, 0x10, 0xB7, 0x83, 0x96, 0xA0, 0x5F, 0x51, 0x6A,
            0x19, 0xC0, 0x8D, 0x0F, 0x20, 0xED, 0xBA, 0x30, 0x2B, 0x65, 0x79, 0x9D, 0x6A, 0x28,
            0xE9, 0x78, 0xA9, 0xFA
        }), 1);
        var fileB = new Signature(
            new Hash("SHA256", new byte[]
        {
            0xE7, 0xB1, 0xBE, 0xE0, 0xA2, 0x14, 0xCB, 0xAF, 0xA3, 0xB2, 0x5B, 0xB8, 0xF7, 0x3A,
            0x40, 0xC2, 0xA0, 0xAA, 0x78, 0x7D, 0x3B, 0x03, 0x63, 0x5A, 0xC5, 0xB7, 0x7B, 0x7A,
            0xC4, 0x93, 0x2A, 0x7B
        }
                    ),
        1L);
        var fileC = new Signature(
            new Hash("SHA256", new byte[]
        {
            0x62, 0xDC, 0x30, 0x20, 0x8A, 0xB5, 0x29, 0xAB, 0xF2, 0xB3, 0x8A, 0x5B, 0x90, 0x45,
            0xBC, 0x1A, 0x2E, 0xCE, 0x24, 0x77, 0xED, 0x7D, 0x34, 0xF4, 0x1C, 0xF9, 0x81, 0xAE,
            0xA9, 0x1A, 0x80, 0xA9
        }
                    ),
        1L);
        var expected = new RepoWithFileChunks(new Domain.Entities.Repo.Repo("repo",
                                              new List<Modpack>
        {
            new Modpack
            {
                Name = "Default",
                Mods = new List<Mod>
                {
                    new Mod
                    {
                        Name = "mod1",
                        Files = new List<RelativePathFile>
                        {
                            new RelativePathFile
                            {Signature = fileA, RelativePath = PurePath.Create("a.txt")},
                            new RelativePathFile
                            {Signature = fileB, RelativePath = PurePath.Create("b.txt")}
                        }
                    },
                    new Mod
                    {
                        Name = "mod2",
                        Files = new List<RelativePathFile>
                        {
                            new RelativePathFile
                            {Signature = fileC, RelativePath = PurePath.Create("c.txt")}
                        }
                    }
                }
            }
        }, "a"),
        new List<FileChunks>
        {
            new FileChunks(
                new Signature(
                    new Hash("SHA256", new byte[]
            {
                0x16, 0xD2, 0x1A, 0xE1, 0xF9, 0xBF, 0x10, 0xB7, 0x83, 0x96, 0xA0, 0x5F, 0x51, 0x6A,
                0x19, 0xC0, 0x8D, 0x0F, 0x20, 0xED, 0xBA, 0x30, 0x2B, 0x65, 0x79, 0x9D, 0x6A, 0x28,
                0xE9, 0x78, 0xA9, 0xFA
            }), 1),
            new Chunk[1]
            {
                new Chunk
                {
                    Position = 0,
                    Signature = new Signature(
                        new Hash("SHA256", new byte[]
                    {
                        0xCA, 0x97, 0x81, 0x12, 0xCA, 0x1B, 0xBD, 0xCA, 0xFA, 0xC2, 0x31, 0xB3, 0x9A,
                        0x23, 0xDC, 0x4D, 0xA7, 0x86, 0xEF, 0xF8, 0x14, 0x7C, 0x4E, 0x72, 0xB9, 0x80,
                        0x77, 0x85, 0xAF, 0xEE, 0x48, 0xBB
                    }), 1)
                }
            }),
            new FileChunks(
                new Signature(
                    new Hash("SHA256", new byte[]
            {
                0xE7, 0xB1, 0xBE, 0xE0, 0xA2, 0x14, 0xCB, 0xAF, 0xA3, 0xB2, 0x5B, 0xB8, 0xF7, 0x3A,
                0x40, 0xC2, 0xA0, 0xAA, 0x78, 0x7D, 0x3B, 0x03, 0x63, 0x5A, 0xC5, 0xB7, 0x7B, 0x7A,
                0xC4, 0x93, 0x2A, 0x7B
            }), 1),
            new Chunk[1]
            {
                new Chunk
                {
                    Position = 0,
                    Signature = new Signature(
                        new Hash("SHA256", new byte[]
                    {
                        0x3E, 0x23, 0xE8, 0x16, 0x00, 0x39, 0x59, 0x4A, 0x33, 0x89, 0x4F, 0x65, 0x64,
                        0xE1, 0xB1, 0x34, 0x8B, 0xBD, 0x7A, 0x00, 0x88, 0xD4, 0x2C, 0x4A, 0xCB, 0x73,
                        0xEE, 0xAE, 0xD5, 0x9C, 0x00, 0x9D
                    }), 1)
                }
            }),
            new FileChunks(
                new Signature(
                    new Hash("SHA256", new byte[]
            {
                0x62, 0xDC, 0x30, 0x20, 0x8A, 0xB5, 0x29, 0xAB, 0xF2, 0xB3, 0x8A, 0x5B, 0x90, 0x45,
                0xBC, 0x1A, 0x2E, 0xCE, 0x24, 0x77, 0xED, 0x7D, 0x34, 0xF4, 0x1C, 0xF9, 0x81, 0xAE,
                0xA9, 0x1A, 0x80, 0xA9
            }), 1),
            new Chunk[1]
            {
                new Chunk
                {
                    Position = 0,
                    Signature = new Signature(
                        new Hash("SHA256", new byte[]
                    {
                        0x2E, 0x7D, 0x2C, 0x03, 0xA9, 0x50, 0x7A, 0xE2, 0x65, 0xEC, 0xF5, 0xB5, 0x35,
                        0x68, 0x85, 0xA5, 0x33, 0x93, 0xA2, 0x02, 0x9D, 0x24, 0x13, 0x94, 0x99, 0x72,
                        0x65, 0xA1, 0xA2, 0x5A, 0xEF, 0xC6
                    }), 1)
                }
            })
        });
        actual.Should().BeEquivalentTo(expected);
    }
}
}
