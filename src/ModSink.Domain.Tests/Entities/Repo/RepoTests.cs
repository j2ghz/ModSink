using FluentAssertions;
using Xunit;

namespace ModSink.Domain.Tests.Entities.Repo
{
    public class RepoTests
    {
        [Fact]
        public void RepoEquals()
        {
            var repo = new Domain.Entities.Repo.Repo();
            var clone = new Domain.Entities.Repo.Repo();
            repo.Should().Be(clone);
            repo.Should().BeEquivalentTo(clone);
        }
    }
}