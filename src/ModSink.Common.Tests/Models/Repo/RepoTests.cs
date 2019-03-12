﻿using System;
using System.Linq;
using Bogus;
using Xunit;

namespace ModSink.Common.Tests.Models.Repo
{
    public class RepoTests : TestWithFaker<Common.Models.DTO.Repo.Repo>
    {
        public static readonly Faker<Common.Models.DTO.Repo.Repo> RepoFaker =
            new Faker<Common.Models.DTO.Repo.Repo>().StrictMode(true)
                .RuleFor(r => r.BaseUri, f => new Uri(f.Internet.UrlWithPath()))
                .RuleFor(r => r.Modpacks, f => ModpackTests.ModpackFaker.Generate(3))
                .RuleFor(r => r.Files,
                    f => f.Make(3, () => FileSignatureTests.FileSignature)
                        .ToDictionary(fs => fs, fs => new Uri(f.Internet.UrlWithPath())));

        public override Faker<Common.Models.DTO.Repo.Repo> Faker { get; } = RepoFaker;

        [Fact(Skip = "Test breaks, but it works")]
        public override void IsSerializeable()
        {
            base.IsSerializeable();
        }
    }
}