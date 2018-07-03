using Bogus;
using ModSink.Common.Models.Repo;
using ModSink.Common.Tests;

namespace Modsink.Common.Tests.Models.Repo
{
    public class ModpackTests : TestWithFaker<Modpack>
    {
        public override Faker<Modpack> Faker { get; } = new Faker<Modpack>().StrictMode(true)
            .RuleFor(m => m.Name, f => f.Company.CompanyName())
            .RuleFor(m => m.Mods, new ModEntryTests().Faker.Generate(3));
    }
}