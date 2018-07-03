using Bogus;
using FluentAssertions;
using ModSink.Common.Models.Repo;
using Xunit;

namespace Modsink.Common.Tests.Models.Repo
{
    public class ModEntryTests
    {
        public static readonly Faker<ModEntry> ModEntryFaker =
            new Faker<ModEntry>().StrictMode(true)
                .RuleFor(m => m.Mod, ModTests.ModFaker.Generate())
                .RuleForType(typeof(bool), f => f.Random.Bool());

        [Fact]
        public void HasValidFaker()
        {
            ModEntryFaker.AssertConfigurationIsValid();
        }


        [Fact]
        public void IsSerializeable()
        {
            Assert.All(ModEntryFaker.Generate(5), modEntry => { modEntry.Should().BeBinarySerializable(); });
        }
    }
}