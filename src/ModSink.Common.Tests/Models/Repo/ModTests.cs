using System;
using System.IO;
using System.Linq;
using Bogus;
using ModSink.Common.Models.Repo;
using ModSink.Common.Tests;

namespace Modsink.Common.Tests.Models.Repo
{
    public class ModTests : TestWithFaker<Mod>
    {
        public static readonly Faker<Mod> ModFaker =
            new Faker<Mod>().StrictMode(true)
                .RuleFor(m => m.Version, f => f.System.Semver())
                .RuleFor(m => m.Name, f => f.System.FileName())
                .RuleFor(m => m.Files,
                    f => f.Make(3,
                        () => new Tuple<Uri, FileSignature>(new Uri(Path.GetFullPath(f.System.FilePath())),
                            FileSignatureTests.FileSignature)).ToDictionary(t => t.Item1, t => t.Item2));

        public override Faker<Mod> Faker { get; } = ModFaker;
    }
}