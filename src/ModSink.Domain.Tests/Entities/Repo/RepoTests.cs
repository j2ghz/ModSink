using System.Collections.Generic;
using FluentAssertions;
using ModSink.Domain.Entities.File;
using ModSink.Domain.Entities.Repo;
using PathLib;
using Xunit;

namespace ModSink.Domain.Tests.Entities.Repo
{
    public class RepoTests
    {
        [Fact]
        public void RepoEquals()
        {
            var repo = new Domain.Entities.Repo.Repo("", new List<Modpack>(),
                new Dictionary<FileSignature, IPurePath>());
            var clone = new Domain.Entities.Repo.Repo("", new List<Modpack>(),
                new Dictionary<FileSignature, IPurePath>());
            repo.Should().Be(clone);
            repo.Should().BeEquivalentTo(clone);
        }
    }
}