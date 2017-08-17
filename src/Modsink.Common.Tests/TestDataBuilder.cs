using FizzWare.NBuilder;
using ModSink.Core.Models.Repo;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Modsink.Common.Tests
{
    public static class TestDataBuilder
    {
        public static Repo Repo()
        {
            var hashes = Builder<HashValue>.CreateListOfSize(100).Build();

            var filehashes = hashes.ToDictionary(h => h, _ => Builder<string>.CreateNew().Build());

            var mods = Builder<Mod>.CreateListOfSize(100)
                .All()
                .With(m => m.Files = Pick.UniqueRandomList.From

            var modpacks = Builder<Modpack>.CreateListOfSize(100)
                .All()
                    .With(m => m.Mods = null);

            return Builder<Repo>
                .CreateNew()
                .With(r => r.Files = filehashes)
                .With(r => r.Modpacks = modpacks)
                .Build();
        }
    }
}