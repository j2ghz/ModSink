using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using ModSink.Application.Hashing;
using ModSink.Infrastructure.Hashing;
using Moq;
using Xunit;

namespace ModSink.Infrastructure.Tests.Hashing
{
    public class HashingServiceTests
    {
        [Fact
        ]
        public async Task GetFileHashes()
        {
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"/repo/mod1/a.txt", new MockFileData("a")},
                {@"/repo/mod1/b.txt", new MockFileData("b")},
                {@"/repo/mod2/c.txt", new MockFileData("c")}
            });
            var hashFunction = new Mock<IHashFunction>();

            var hashingService = new HashingService(hashFunction.Object,
                Options.Create(new HashingService.Options()),
                new FileStreamOpener(Options.Create(new FileStreamOpener.Options()),
                    new MockFileStreamFactory(fs)));

            await foreach (var relativeUriFile in hashingService.GetFileHashes(
                fs.DirectoryInfo.FromDirectoryName(@"/repo/"), CancellationToken.None))
                relativeUriFile.Should().NotBeNull();
        }
    }
}