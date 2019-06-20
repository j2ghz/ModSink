using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModSink.Application;
using ModSink.Application.Hashing;
using ModSink.Domain.Entities.File;
using ModSink.Domain.Entities.Repo;

namespace ModSink.Infrastructure.RepoBuilders
{
    public class ModsInRepoRootBuilder : IRepoBuilder<ModsInRepoRootBuilderConfig>
    {
        private readonly IHashingService _hashingService;

        public ModsInRepoRootBuilder(IHashingService hashingService)
        {
            _hashingService = hashingService;
        }

        public async Task<Repo> Build(IDirectoryInfo root, CancellationToken token)
        {
            return await Build(root,
                null, token);
        }

        public async Task<Repo> Build(IDirectoryInfo root, ModsInRepoRootBuilderConfig config, CancellationToken token)
        {
            if (config == null)
                config = new ModsInRepoRootBuilderConfig
                {
                    Name = root.Name,
                    Modpacks =
                    {
                        new ModsInRepoRootBuilderConfig.Modpack
                            {Name = "Default", Mods = root.GetDirectories().Select(d => d.Name).ToList()}
                    }
                };

            var repoFiles = new Dictionary<FileSignature, RelativeUriFile>();
            var allModNames = config.Modpacks.SelectMany(m => m.Mods).Distinct();
            var builtMods = new List<Mod>();
            var modDirs = root.GetDirectories();
            foreach (var modName in allModNames)
            {
                var modDir = modDirs.First(d => d.Name == modName);
                var modFiles = new List<RelativeUriFile>();
                await foreach (var modFile in _hashingService.GetFileHashes(modDir, token))
                {
                    modFiles.Add(modFile);
                    repoFiles.Add(modFile.Signature, modFile);
                }

                builtMods.Add(new Mod {Files = modFiles, Name = modName});
            }

            var modpacks = config.Modpacks.Select(modpack => new Modpack
            {
                Name = modpack.Name,
                Mods = modpack.Mods.Select(modName => builtMods.First(mod => mod.Name == modName)).ToList()
            });

            var repo = new Repo {Name = config.Name, Modpacks = modpacks.ToList(), Files = repoFiles.Values};
            return repo;
        }
    }

    public class ModsInRepoRootBuilderConfig
    {
        public string Name { get; set; }
        public ICollection<Modpack> Modpacks { get; set; }

        public class Modpack
        {
            public string Name { get; set; }
            public ICollection<string> Mods { get; set; }
        }
    }
}