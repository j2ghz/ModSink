using Bogus;
using ModSink.Common.Models.Repo;
using ModSink.Common.Tests;

namespace Modsink.Common.Tests.Models.Repo
{
    public class ModpackTests : TestWithFaker<Modpack>
    {
        public static readonly Faker<Modpack> ModpackFaker = new Faker<Modpack>().StrictMode(true)
            .RuleFor(m => m.Name, f => f.Company.CompanyName())
            .RuleFor(m => m.Mods, ModEntryTests.ModEntryFaker.Generate(3))
            .RuleFor(m=>m.Selected,f=>f.Random.Bool());

        public override Faker<Modpack> Faker { get; } = ModpackFaker;
    }
}