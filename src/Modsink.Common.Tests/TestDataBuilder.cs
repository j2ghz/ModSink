using ModSink.Core.Models.Repo;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Bogus;
using System;

namespace Modsink.Common.Tests
{
    public static class TestDataBuilder
    {
        public static Repo Repo()
        {
            var files = new Faker<Tuple<string, HashValue>>()
                .CustomInstantiator(f => new Tuple<string, HashValue>(f.System.FileName(), new HashValue(f.Random.Bytes(8))))
                .Generate(1000).ToDictionary(a => a.Item1, a => a.Item2);

            var mods = new Faker<Mod>()
                .RuleFor(m => m.Files, f => f.PickRandom(files, 100).ToDictionary(kvp => kvp.Key, kvp => kvp.Value))
                .RuleFor(m => m.Name, f => f.System.CommonFileName())
                .RuleFor(m => m.Version, f => f.System.Semver())
                .Generate(100);

            var modpacks = new Faker<Modpack>()
                .RuleFor(mp => mp.Mods, f => f.PickRandom(mods, 50).ToDictionary(a => a, a => ModFlags.None))
                .Generate(10).ToList();

            return new Faker<Repo>()
                .RuleFor(r => r.Files, _ => files.ToDictionary(kp => kp.Value, kp => kp.Key))
                .RuleFor(r => r.Modpacks, _ => modpacks)
                .Generate();
        }
    }
}