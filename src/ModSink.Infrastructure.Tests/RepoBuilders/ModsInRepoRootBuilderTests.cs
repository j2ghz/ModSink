using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using ModSink.Infrastructure.Hashing;
using ModSink.Infrastructure.RepoBuilders;
using ModSink.Infrastructure.Tests.Hashing;
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
                new FileStreamOpener(Options.Create(new FileStreamOpener.Options()),
                    new MockFileStreamFactory(fileSystem)));
            var builder = new ModsInRepoRootBuilder(hashingService);
            var root = fileSystem.DirectoryInfo.FromDirectoryName(@"/repo/");

            var repo = await builder.Build(root,
                CancellationToken.None);

            repo.Name.Should().Be("repo");
            repo.Files.Should().HaveCount(3);
            repo.Modpacks.Should().HaveCount(1);
            repo.Modpacks.Single().Mods.Select(m => m.Name).Should().Contain(new[] {"mod1", "mod2"});
        }
    }
}