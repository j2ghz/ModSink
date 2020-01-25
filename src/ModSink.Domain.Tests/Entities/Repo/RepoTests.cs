using System.Collections.Generic;
using FluentAssertions;
using ModSink.Domain.Entities.File;
using ModSink.Domain.Entities.Repo;
using PathLib;
using Xunit;

namespace ModSink.Domain.Tests.Entities.Repo {
  public class RepoTests {
    [Fact]
    public void
    RepoEquivalent() {
      var repo = new Domain.Entities.Repo.Repo("", new List<Modpack>(), "");
      var clone = new Domain.Entities.Repo.Repo("", new List<Modpack>(), "");
      repo.Should().BeEquivalentTo(clone);
    }
  }
}