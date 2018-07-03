using Bogus;
using ModSink.Common.Models.Repo;
using ModSink.Common.Tests;

namespace Modsink.Common.Tests.Models.Repo
{
    public class ModEntryTests : TestWithFaker<ModEntry>
    {
        public override Faker<ModEntry> Faker { get; } =
            new Faker<ModEntry>().StrictMode(true)
                .RuleFor(m => m.Mod, ModTests.ModFaker.Generate())
                .RuleForType(typeof(bool), f => f.Random.Bool());
    }
}