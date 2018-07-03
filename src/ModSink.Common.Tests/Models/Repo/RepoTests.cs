using System;
using System.Linq;
using Bogus;
using ModSink.Common.Tests;

namespace Modsink.Common.Tests.Models.Repo
{
    public class RepoTests : TestWithFaker<ModSink.Common.Models.Repo.Repo>
    {
        public static readonly Faker<ModSink.Common.Models.Repo.Repo> RepoFaker =
            new Faker<ModSink.Common.Models.Repo.Repo>().StrictMode(true)
                .RuleFor(r => r.BaseUri, f => new Uri(f.Internet.UrlWithPath()))
                .RuleFor(r=>r.Modpacks,f=>ModpackTests.ModpackFaker.Generate(3))
                .RuleFor(r=>r.Files,f=>f.Make(5,()=>FileSignatureTests.FileSignature).ToDictionary(fs=>fs,fs=>new Uri(f.Internet.UrlWithPath())));

        public override Faker<ModSink.Common.Models.Repo.Repo> Faker { get; } = RepoFaker;
    }
}