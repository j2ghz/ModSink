using System;
using System.Linq;
using Bogus;
using Xunit;

namespace ModSink.Common.Tests.Models.Repo
{
    public class RepoTests : TestWithFaker<Common.Models.Repo.Repo>
    {
        public static readonly Faker<Common.Models.Repo.Repo> RepoFaker =
            new Faker<Common.Models.Repo.Repo>().StrictMode(true)
                .RuleFor(r => r.BaseUri, f => new Uri(f.Internet.UrlWithPath()))
                .RuleFor(r => r.Modpacks, f => ModpackTests.ModpackFaker.Generate(3))
                .RuleFor(r => r.Files,
                    f => f.Make(3, () => FileSignatureTests.FileSignature)
                        .ToDictionary(fs => fs, fs => new Uri(f.Internet.UrlWithPath())));

        public override Faker<Common.Models.Repo.Repo> Faker { get; } = RepoFaker;

        [Fact]
        public override void IsSerializeable()
        {
            base.IsSerializeable();
        }
    }
}