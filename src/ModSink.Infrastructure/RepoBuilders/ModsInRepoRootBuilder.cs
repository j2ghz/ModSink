using System;
using System.IO.Abstractions;
using ModSink.Application;
using ModSink.Application.Hashing;
using ModSink.Domain.Entities.Repo;

namespace ModSink.Infrastructure.RepoBuilders
{
    public class ModsInRepoRootBuilder : IRepoBuilder<Config>
    {
        public class Config
        {
            public string Name { get; private set; }
            public ICollection<Modpack> Modpacks { get; private set; }
            public class Modpack
            {
                public string Name { get; private set; }
                public ICollection<string> Mods { get; private set; }
            }
        }
        private readonly IHashingService _hashingService;

        public ModsInRepoRootBuilder(IHashingService hashingService)
        {
            _hashingService = hashingService;
        }

        public Repo Build(IDirectoryInfo root, Config config)
        {
            //Read config

            //hash requested mods
            var allMods = config.Modpacks.SelectMany(m => m.Mods);
            var modFolders = root.
            //build modpacks
            var modpacks = config.Modpacks.Select(m => new Modpack)
            //build repo
            var repo = new Repo() { nameof = config.Name, };
            throw new NotImplementedException();
        }
    }
}