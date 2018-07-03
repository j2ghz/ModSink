using System;
using System.IO;
using System.Linq;
using Bogus;
using FluentAssertions;
using ModSink.Common.Models.Repo;
using Xunit;

namespace Modsink.Common.Tests.Models.Repo
{
    public class ModTests
    {
        public static readonly Faker<Mod> ModFaker =
            new Faker<Mod>().StrictMode(true)
                .RuleFor(m => m.Version, f => f.System.Semver())
                .RuleFor(m => m.Name, f => f.System.FileName())
                .RuleFor(m => m.Files,
                    f => f.Make(10,
                        () => new Tuple<Uri, FileSignature>(new Uri(Path.GetFullPath(f.System.FilePath())),
                            FileSignatureTests.FileSignature)).ToDictionary(t => t.Item1, t => t.Item2));

        [Fact]
        public void HasValidFaker()
        {
            ModFaker.AssertConfigurationIsValid();
        }


        [Fact]
        public void IsSerializeable()
        {
            Assert.All(ModFaker.Generate(5), mod => { mod.Should().BeBinarySerializable(); });
        }
    }
}