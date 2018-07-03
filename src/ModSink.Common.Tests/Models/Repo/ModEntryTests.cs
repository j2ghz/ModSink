using Bogus;
using ModSink.Common.Models.Repo;
using ModSink.Common.Tests;

namespace Modsink.Common.Tests.Models.Repo
{
    public class ModEntryTests : TestWithFaker<ModEntry>
    {
        public static readonly Faker<ModEntry> ModEntryFaker =
            new Faker<ModEntry>().StrictMode(true)
                .RuleFor(m => m.Mod, ModTests.ModFaker.Generate())
                .RuleForType(typeof(bool), f => f.Random.Bool());

        public override Faker<ModEntry> Faker { get; } = ModEntryFaker;
    }
}