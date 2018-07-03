using System;
using Bogus;
using FluentAssertions;
using Xunit;

namespace Modsink.Common.Tests.Models.Repo
{
    public class RepoTests
    {
        [Fact]
        public void IsSerializable()
        {
            var repoFaker = new Faker<ModSink.Common.Models.Repo.Repo>()
                .StrictMode(true)
                .RuleFor(r => r.BaseUri, f => new Uri(f.Internet.UrlWithPath()));
            repoFaker.AssertConfigurationIsValid();
            Assert.All(repoFaker.Generate(5), r => { r.Should().BeBinarySerializable(); });
        }
    }
}