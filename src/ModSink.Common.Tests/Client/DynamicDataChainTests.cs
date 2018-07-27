using System;
using Bogus;
using DynamicData;
using FluentAssertions;
using ModSink.Common.Client;
using ModSink.Common.Models.Repo;
using Xunit;

namespace ModSink.Common.Tests.Client
{
    public class DynamicDataChainTests
    {
        [Fact]
        public void GetModpacksFromReposTest()
        {
            var faker = new Faker();
            var repos = new SourceCache<Repo, Uri>(r=>r.BaseUri);
            var modpacks = DynamicDataChain.GetModpacksFromRepos(repos);
            modpacks.Count.Should().Be(0);
            repos.AddOrUpdate(new Repo(){BaseUri = new Uri(faker.Internet.UrlWithPath()) });
            modpacks.Count.Should().Be(1);
        }
    }
}