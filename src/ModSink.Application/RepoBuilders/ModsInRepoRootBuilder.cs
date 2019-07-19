using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModSink.Application.Hashing;
using ModSink.Domain.Entities.File;
using ModSink.Domain.Entities.Repo;
using PathLib;

namespace ModSink.Application.RepoBuilders
{
    public class ModsInRepoRootBuilder : IRepoBuilder<ModsInRepoRootBuilderConfig>
    {
        private readonly IHashingService _hashingService;

        public ModsInRepoRootBuilder(IHashingService hashingService)
        {
            _hashingService = hashingService;
        }

        public Task<RepoWithFileChunks> Build(IDirectoryInfo root, CancellationToken token)
        {
            return Build(root,
                null, token);
        }

        public async Task<RepoWithFileChunks> Build(IDirectoryInfo root, ModsInRepoRootBuilderConfig? config, CancellationToken token)
        {
            if (config == null)
                config = new ModsInRepoRootBuilderConfig(root.Name,
                    new List<ModsInRepoRootBuilderConfig.Modpack>
                    {
                        new ModsInRepoRootBuilderConfig.Modpack("Default",
                            root.GetDirectories().Select(d => d.Name).ToList())
                    });

            var repoFiles = new Dictionary<FileSignature, IPurePath>();
            var repoChunks = new HashSet<FileChunks>();
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
                    var (file, chunks) = await modFileTask;
                    var repoFile = file.InDirectory(PurePath.Create(modDir.Name));
                    repoFiles[file.Signature] = repoFile.RelativePath;
                    repoChunks.Add(new FileChunks(file.Signature, chunks));
                }

                builtMods.Add(new Mod {Files = modFiles.Select(t => t.Result.File).ToList(), Name = modName});
            }

            var modpacks = config.Modpacks.Select(modpack => new Modpack
            {
                Name = modpack.Name,
                Mods = modpack.Mods.Select(modName => builtMods.First(mod => mod.Name == modName)).ToList()
            });

            var repo = new Repo(config.Name, modpacks.ToList(), repoFiles);
            return new RepoWithFileChunks(repo,repoChunks);
        }
    }

    public class ModsInRepoRootBuilderConfig
    {
        public ModsInRepoRootBuilderConfig(string name, IEnumerable<Modpack> modpacks)
        {
            Name = name;
            Modpacks = modpacks.ToList();
        }

        public ICollection<Modpack> Modpacks { get; }

        public string Name { get; }

        public class Modpack
        {
            public Modpack(string name, IEnumerable<string> mods)
            {
                Name = name;
                Mods = mods.ToList();
            }

            public ICollection<string> Mods { get; }

            public string Name { get; }
        }
    }
}