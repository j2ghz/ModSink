using System;
using Bogus;
using ModSink.Common.Models.DTO.Group;

namespace ModSink.Common.Tests.Models.Group
{
    public class RepoInfoTests : TestWithFaker<RepoInfo>
    {
        public static Faker<RepoInfo> RepoInfoFaker = new Faker<RepoInfo>().StrictMode(true)
            .RuleFor(r => r.Uri, f => new Uri(f.Internet.UrlWithPath()));

        public override Faker<RepoInfo> Faker { get; } = RepoInfoFaker;
    }
}