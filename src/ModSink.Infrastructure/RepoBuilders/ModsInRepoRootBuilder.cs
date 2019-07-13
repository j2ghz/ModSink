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

        public Task<Repo> Build(IDirectoryInfo root, CancellationToken token)
        {
            return Build(root,
                null, token);
        }

        public async Task<Repo> Build(IDirectoryInfo root, ModsInRepoRootBuilderConfig? config, CancellationToken token)
        {
            if (config == null)
                config = new ModsInRepoRootBuilderConfig(root.Name,
                    new List<ModsInRepoRootBuilderConfig.Modpack>
                    {
                        new ModsInRepoRootBuilderConfig.Modpack("Default",
                            root.GetDirectories().Select(d => d.Name).ToList())
                    });

            var repoFiles = new Dictionary<FileSignature, RelativeUriFile>();
            var allModNames = config.Modpacks.SelectMany(m => m.Mods).Distinct();
            var builtMods = new List<Mod>();
            var modDirs = root.GetDirectories();
            foreach (var modName in allModNames)
            {
                var modDir = modDirs.First(d => d.Name == modName);

                var modFiles = _hashingService.GetFileHashes(modDir, token).ToList();
                await Task.WhenAll(modFiles);
                foreach (var modFileTask in modFiles)
                {
                    var modFile = await modFileTask;
                    var repoFile = modFile.InDirectory(modDir.Name);
                    repoFiles[modFile.Signature] = repoFile;
                }

                builtMods.Add(new Mod {Files = modFiles.Select(t => t.Result).ToList(), Name = modName});
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
        public ModsInRepoRootBuilderConfig(string name, IEnumerable<Modpack> modpacks)
        {
            Name = name;
            Modpacks = modpacks.ToList();
        }

        public string Name { get; }
        public ICollection<Modpack> Modpacks { get; }

        public class Modpack
        {
            public Modpack(string name, IEnumerable<string> mods)
            {
                Name = name;
                Mods = mods.ToList();
            }

            public string Name { get; }
            public ICollection<string> Mods { get; }
        }
    }
}