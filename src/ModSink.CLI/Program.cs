using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using Humanizer;
using Microsoft.Extensions.CommandLineUtils;
using ModSink.Common;
using ModSink.Common.Client;
using ModSink.Core;
using ModSink.Core.Client;
using ModSink.Core.Models.Group;
using ModSink.Core.Models.Repo;
using Serilog;

namespace ModSink.CLI
{
    public static class Program
    {
        private static void AddColCheck(this CommandLineApplication app)
        {
            app.Command("collcheck", command =>
            {
                command.Description = "Checks files for collisions";
                command.HelpOption("-?|-h|--help");
                var pathArg = command.Argument("[path]", "Path to folder");

                command.OnExecute(() =>
                {
                    var pathStr = pathArg.Value ?? ".";
                    var path = Path.Combine(Directory.GetCurrentDirectory(), pathStr);
                    IHashFunction xxhash = new XXHash64();
                    var hashing = new HashingService(xxhash);
                    hashing.GetFiles(new DirectoryInfo(path))
                        .Select(f =>
                        {
                            var hash = hashing.GetFileHash(f, CancellationToken.None).GetAwaiter().GetResult();
                            Console.WriteLine($"{hash} {Path.GetRelativePath(path, f.FullName)}");
                            return new {f, hash};
                        })
                        .GroupBy(a => a.hash.ToString())
                        .Where(g => g.Count() > 1)
                        .ForEach(g =>
                        {
                            Console.WriteLine(g.Key);
                            foreach (var i in g)
                                Console.WriteLine($"    {i.f.FullName}");
                            Console.WriteLine();
                        });

                    Console.WriteLine("Done.");
                    return 0;
                });
            });
        }

        private static void AddDump(this CommandLineApplication app)
        {
            app.Command("dump", command =>
            {
                command.Description = "Writes out the contents of a repo";
                command.HelpOption("-?|-h|--help");
                var uriArg = command.Argument("[uri]", "Uri to repo to download");

                command.OnExecute(() =>
                {
                    var uriStr = uriArg.Value;
                    var uri = new Uri(uriStr);
                    if (uri.IsFile)
                    {
                        var repo = (Repo) new BinaryFormatter().Deserialize(new FileInfo(uri.LocalPath).OpenRead());
                        repo.BaseUri = new Uri("http://base.uri/repo/");
                        DumpRepo(repo);
                    }
                    else
                    {
                        var downloader = new HttpClientDownloader();
                        var client = new ClientService(new DownloadService(downloader), null, downloader,
                            new BinaryFormatter());

                        Console.WriteLine("Downloading repo");
                        client.GroupUrls.Add(uriStr);
                        foreach (var repo in client.Repos.Items)
                            DumpRepo(repo);
                    }

                    return 0;
                });
            });
        }

        private static void AddCheck(this CommandLineApplication app)
        {
            app.Command("check", command =>
            {
                command.Description =
                    "Checks that every file in the folder is named as its hash and deletes if it's not";
                command.HelpOption("-?|-h|--help");
                var pathArg = command.Argument("[path]", "Path to folder to check");

                command.OnExecute(async () =>
                {
                    var pathStr = pathArg.Value ?? "./hashed";
                    var path = Path.Combine(Directory.GetCurrentDirectory(), pathStr);

                    var hashing = new HashingService(new XXHash64());

                    foreach (var file in hashing.GetFiles(new DirectoryInfo(path)).OrderBy(f => f.Length))
                    {
                        if (file.Length <= 0)
                        {
                            file.Delete();
                            continue;
                        }

                        var size = file.Length.Bytes();
                        var hashFromName = Path.GetFileNameWithoutExtension(file.FullName);
                        Console.Write($"{size.Humanize("#").PadLeft(10)}: {hashFromName} | ");
                        HashValue hash;
                        using (var src = file.OpenRead())
                        {
                            var start = DateTime.Now;
                            hash = await hashing.GetFileHash(src, CancellationToken.None);
                            var speed = size.Per(DateTime.Now - start);
                            Console.Write($"{hash} | {speed.Humanize("#").PadLeft(10)} | ");
                        }

                        if (hash.ToString() != hashFromName)
                        {
                            Console.Write("DELETE");
                            file.Delete();
                        }

                        Console.WriteLine();
                    }

                    return 0;
                });
            });
        }

        private static void AddImport(this CommandLineApplication app)
        {
            app.Command("import", command =>
            {
                command.Description = "Copies every file in the folder and renames it to its hash";
                command.HelpOption("-?|-h|--help");
                var pathArg = command.Argument("[path]", "Path to folder to look for files in");
                var pathDestArg = command.Argument("[dest path]", "Path in which to copy the files");

                command.OnExecute(async () =>
                {
                    var pathStr = pathArg.Value ?? ".";
                    var pathDestStr = pathDestArg.Value ?? "./hashed";
                    var path = Path.Combine(Directory.GetCurrentDirectory(), pathStr);
                    var pathDest = Path.Combine(Directory.GetCurrentDirectory(), pathDestStr);

                    var hashing = new HashingService(new XXHash64());

                    foreach (var file in hashing.GetFiles(new DirectoryInfo(path)))
                    {
                        if (file.Length <= 0) continue;
                        var size = file.Length.Bytes();
                        Console.Write($"{size.Humanize("#").PadLeft(10)}: ");
                        using (var src = file.OpenRead())
                        {
                            var start = DateTime.Now;
                            var hash = await hashing.GetFileHash(src, CancellationToken.None);
                            var filehash = new FileWithHash(file, hash);
                            var speed = size.Per(DateTime.Now - start);
                            Console.Write($"{hash} {speed.Humanize("#").PadLeft(10)}(hash) | ");
                            var fileDest = Path.Combine(pathDest, filehash.Hash.ToString());
                            try
                            {
                                if (new FileInfo(fileDest).Exists)
                                    Console.Write($"                 | {file.FullName}");
                                else
                                    using (var dest = new FileStream(fileDest, FileMode.Create, FileAccess.Write))
                                    {
                                        start = DateTime.Now;
                                        src.Position = 0;
                                        await src.CopyToAsync(dest);
                                        Console.Write(
                                            $"{size.Per(DateTime.Now - start).Humanize("#").PadLeft(10)}(copy) | {file.FullName}");
                                    }
                            }
                            catch (IOException ex)
                            {
                                Console.WriteLine();
                                Console.WriteLine(ex.ToString());
                            }
                        }

                        Console.WriteLine();
                    }

                    return 0;
                });
            });
        }

        private static void AddSampleRepo(this CommandLineApplication app)
        {
            app.Command("sampleRepo", command =>
            {
                command.Description = "Makes each subfolder of a given folder a mod";
                command.HelpOption("-?|-h|--help");
                var pathArg = command.Argument("[path]", "Path to the folder with mods in separate folder");

                command.OnExecute(async () =>
                {
                    var path = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), pathArg.Value ?? "."));

                    var group = new Group {RepoInfos = new List<RepoInfo>()};
                    foreach (var directory in path.EnumerateDirectories())
                    {
                        var pathUri = new Uri(directory.FullName);
                        var repo = await CreateRepo(directory);
                        repo.BaseUri = new Uri(directory.FullName);
                        var fileName = Path.Combine(directory.FullName, "repo.bin");
                        using (var stream = new FileInfo(fileName).Create())
                        {
                            new BinaryFormatter().Serialize(stream, repo);
                        }

                        var repoInfo = new RepoInfo {Uri = pathUri.MakeRelativeUri(new Uri(fileName))};
                        group.RepoInfos.Add(repoInfo);

                        DumpRepo(repo);
                        Console.WriteLine($"Written to {fileName}");
                    }

                    DumpGroup(group);
                    var fileNameG = Path.Combine(path.FullName, "group.bin");
                    using (var stream = new FileInfo(fileNameG).Create())
                    {
                        new BinaryFormatter().Serialize(stream, group);
                    }

                    Console.WriteLine($"Written to {fileNameG}");

                    return 0;
                });
            });
        }

        private static async Task<Mod> CreateMod(DirectoryInfo modFolder, Action<FileSignature, Uri> fileAction)
        {
            Console.WriteLine($"Processing {modFolder.FullName}");
            var hashing = new HashingService(new XXHash64());
            var obs = hashing.GetFileHashes(modFolder, CancellationToken.None);
            var mod = new Mod
            {
                Files = new Dictionary<Uri, FileSignature>(),
                Name = modFolder.Name,
                Version = "1.0"
            };
            foreach (var lazy in obs)
            {
                var fileHash = await lazy.Value;
                Console.WriteLine($"Processing {fileHash.File.FullName}");
                var fileSig = new FileSignature(fileHash.Hash, fileHash.File.Length);
                mod.Files.Add(new Uri(modFolder.FullName).MakeRelativeUri(new Uri(fileHash.File.FullName)),
                    fileSig);
                fileAction(fileSig, new Uri(fileHash.File.FullName));
            }

            return mod;
        }

        private static async Task<Modpack> CreateModpack(DirectoryInfo directory, Action<FileSignature, Uri> fileAction)
        {
            var modpack = new Modpack {Name = directory.Name, Mods = new List<ModEntry>()};
            foreach (var modFolder in directory.EnumerateDirectories())
            {
                var mod = await CreateMod(modFolder, fileAction);
                modpack.Mods.Add(new ModEntry {Mod = mod});
            }

            return modpack;
        }

        private static async Task<Repo> CreateRepo(DirectoryInfo directoryInfo)
        {
            var repo = new Repo {Modpacks = new List<Modpack>(), Files = new Dictionary<FileSignature, Uri>()};

            foreach (var modPackFolder in directoryInfo.EnumerateDirectories())
            {
                var modpack = await CreateModpack(modPackFolder,
                    (file, uri) =>
                    {
                        repo.Files.Add(file, new Uri(Path.Combine(directoryInfo.FullName, ".")).MakeRelativeUri(uri));
                    });
                repo.Modpacks.Add(modpack);
            }


            return repo;
        }

        private static void DumpDownloadProgress(IObservable<DownloadProgress> obs, string name)
        {
            obs.Sample(TimeSpan.FromMilliseconds(250))
                .Buffer(2, 1)
                .Subscribe(progList =>
                {
                    var prog = new DownloadProgressCombined(progList.Last(), progList.First());
                    var dbl = prog.Current.Remaining.Bytes / prog.Speed.Size.Bytes;
                    if (double.IsNaN(dbl)) dbl = 0;
                    var eta = prog.Speed.Interval.Multiply(dbl);
                    Console.WriteLine(
                        $"\t{prog.Current.State} {name.PadRight(23)} {prog.Current.Remaining.Humanize("#.#").PadLeft(8)} left @ {prog.Speed.Humanize("#.#").PadLeft(10)} ETA: {eta.Humanize()}");
                }, ex => Console.WriteLine(ex.ToString()), () => Console.WriteLine("Done"));
        }

        private static void DumpGroup(Group group)
        {
            Console.WriteLine($"Group:");
            foreach (var groupRepoInfo in group.RepoInfos) Console.WriteLine($"\tRepo: {groupRepoInfo.Uri}");
        }

        private static void DumpRepo(Repo repo)
        {
            Console.WriteLine($"Repo at {repo.BaseUri}");
            Console.WriteLine("Files:");
            foreach (var file in repo.Files)
                Console.WriteLine($"\t{file.Key} at {new Uri(repo.BaseUri, file.Value)}");

            Console.WriteLine("ModPacks:");
            foreach (var modpack in repo.Modpacks)
            {
                Console.WriteLine($"\tModpack '{modpack.Name}'");
                Console.WriteLine("\tMods:");
                foreach (var mod in modpack.Mods)
                    Console.WriteLine($"\t\tMod: '{mod.Mod.Name}' [{mod.Mod.Files.Count} files]");
            }
        }

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate:
                    "{Timestamp:HH:mm:ss} {Level:u3} [{SourceContext}] {Properties} {Message:lj}{NewLine}{Exception}")
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .CreateLogger();

            var app = new CommandLineApplication
            {
                Name = "modsink",
                FullName = "ModSink.CLI"
            };
            app.HelpOption("-?|-h|--help");
            app.ShortVersionGetter = () => typeof(Program).Assembly.GetName().Version.ToString();

            app.AddColCheck();
            app.AddSampleRepo();
            app.AddImport();
            app.AddDump();
            app.AddCheck();

            app.Execute(args.Length > 0 ? args : new[] {"--help"});
        }
    }
}