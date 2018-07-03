using Bogus;
using ModSink.Common.Models.Repo;
using ModSink.Common.Tests;

namespace Modsink.Common.Tests.Models.Repo
{
    public class ModpackTests : TestWithFaker<Modpack>
    {
        public static Faker<Modpack> ModpackFaker = new Faker<Modpack>().StrictMode(true)
            .RuleFor(m => m.Name, f => f.Company.CompanyName())
            .RuleFor(m => m.Mods, ModEntryTests.ModEntryFaker.Generate(3));

        public override Faker<Modpack> Faker { get; } = ModpackFaker;
    }
}