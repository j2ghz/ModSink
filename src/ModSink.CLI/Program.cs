using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Humanizer;
using Microsoft.Extensions.CommandLineUtils;
using ModSink.Common;
using ModSink.Common.Client;
using ModSink.Core;
using ModSink.Core.Client;
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

        private static void AddDownload(this CommandLineApplication app)
        {
            app.Command("download", command =>
            {
                command.Description = "Downloads a missing files from a repo";
                command.HelpOption("-?|-h|--help");
                var uriArg = command.Argument("[uri]", "Uri to repo to download");
                var pathArg = command.Argument("[path]", "Path to local repo");

                command.OnExecute(async () =>
                {
                    var uriStr = uriArg.Value;
                    var uri = new Uri(uriStr);
                    var localStr = pathArg.Value;
                    var localUri = new Uri(localStr);
                    var downloader = new HttpClientDownloader();
                    var client = new ClientService(new DownloadService(downloader), new LocalStorageService(localUri),
                        downloader, new BinaryFormatter());

                    Console.WriteLine("Downloading repo");
                    var repoDown = client.LoadRepo(uri);
                    DumpDownloadProgress(repoDown, "Repo");
                    await repoDown;
                    foreach (var modpack in client.Modpacks.Items)
                    {
                        Console.WriteLine($"Scheduling {modpack.Name} [{modpack.Mods.Count} mods]");
                        await client.DownloadMissingFiles(modpack);
                    }
                    Console.ReadKey();
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
                        var repoDown = client.LoadRepo(uri);
                        DumpDownloadProgress(repoDown, "Repo");
                        repoDown.Wait();
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
                var pathArg = command.Argument("[path]", "Path to the folder with mods");

                command.OnExecute(async () =>
                {
                    var hashing = new HashingService(new XXHash64());

                    var pathStr = pathArg.Value ?? ".";
                    var path = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), pathStr));
                    var pathUri = new Uri(path.FullName);

                    var files = new Dictionary<FileSignature, Uri>();
                    var mods = new List<ModEntry>();

                    foreach (var modFolder in path.EnumerateDirectories())
                    {
                        Console.WriteLine($"Processing {modFolder.FullName}");
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
                            var fileSig = new FileSignature(fileHash.Hash,fileHash.File.Length);
                            mod.Files.Add(new Uri(modFolder.FullName).MakeRelativeUri(new Uri(fileHash.File.FullName)),
                                fileSig);
                            files.Add(fileSig, pathUri.MakeRelativeUri(new Uri(fileHash.File.FullName)));
                        }
                        mods.Add(new ModEntry {Mod = mod});
                    }

                    var repo = new Repo
                    {
                        Files = files,
                        Modpacks = new List<Modpack> {new Modpack {Mods = mods, Name = "TestModpack"}}
                    };

                    var fileName = Path.Combine(pathUri.LocalPath, "repo.bin");
                    new BinaryFormatter().Serialize(new FileInfo(fileName).Create(), repo);
                    Console.WriteLine($"Written to {fileName}");

                    return 0;
                });
            });
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

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
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
            app.AddDownload();
            app.AddImport();
            app.AddDump();
            app.AddCheck();

            app.Execute(args.Length > 0 ? args : new[] {"--help"});
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
    }
}