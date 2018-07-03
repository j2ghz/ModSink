using System;
using Bogus;

namespace ModSink.Common.Tests.Models.Group
{
    public class GroupTests : TestWithFaker<Common.Models.Group.Group>
    {
        public static Faker<Common.Models.Group.Group> GroupFaker = new Faker<Common.Models.Group.Group>()
            .StrictMode(true)
            .RuleFor(g => g.BaseUri, f => new Uri(f.Internet.UrlWithPath()))
            .RuleFor(g => g.RepoInfos, _ => RepoInfoTests.RepoInfoFaker.Generate(3));

        public override Faker<Common.Models.Group.Group> Faker { get; } = GroupFaker;
    }
}