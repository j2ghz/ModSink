using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using ModSink.Application.Hashing;
using ModSink.Domain.Entities.Repo;
using ModSink.Infrastructure.RepoBuilders;
using Moq;
using Xunit;

namespace ModSink.Infrastructure.Tests.RepoBuilders
{
    public class ModsInRepoRootBuilderTests
    {
        [Fact]
        public async Task WithoutConfig()
        {
            var hashingService = new Mock<IHashingService>();
            var builder = new ModsInRepoRootBuilder(hashingService.Object);
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"/repo/mod1/a.txt", new MockFileData("a")},
                {@"/repo/mod1/b.txt", new MockFileData("b")},
                {@"/repo/mod2/c.txt", new MockFileData("c")}
            });
            var root = fileSystem.DirectoryInfo.FromDirectoryName(@"/repo/");
            var repo = await builder.Build(root,
                CancellationToken.None);
            repo.Name.Should().Be("repo");
            repo.Files.Should().HaveCount(3)
                ;

            repo.Modpacks.Should().HaveCount(1).And.Contain(new Modpack());
        }
    }
}