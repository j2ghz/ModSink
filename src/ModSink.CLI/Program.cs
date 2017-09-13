using Microsoft.Extensions.CommandLineUtils;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using ModSink.Common;
using System.Threading;
using ModSink.Core;
using ModSink.Core.Models.Repo;
using System.IO.MemoryMappedFiles;
using System.Reactive.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using ModSink.Core.Client;
using ModSink.Common.Client;
using Humanizer;

namespace ModSink.CLI
{
    public static class Program
    {
        public static void AddColCheck(this CommandLineApplication app)
        {
            app.Command("collcheck", (command) =>
            {
                command.Description = "Checks files for collisions";
                command.HelpOption("-?|-h|--help");
                var pathArg = command.Argument("[path]", "Path to folder");

                command.OnExecute(() =>
                {
                    var pathStr = pathArg.Value ?? ".";
                    var path = Path.Combine(Directory.GetCurrentDirectory(), pathStr);
                    IHashFunction xxhash = new XXHash64();
                    var hashing = new Hashing(xxhash);
                    hashing.GetFiles(new DirectoryInfo(path))
                    .Select(f =>
                    {
                        var hash = ComputeHash(f, xxhash);
                        return new { f, hash };
                    })
                    .GroupBy(a => a.hash.ToString())
                    .Where(g => g.Count() > 1)
                    .ForEach(g =>
                    {
                        Console.WriteLine(g.Key);
                        foreach (var i in g)
                        {
                            Console.WriteLine($"    {i.f.FullName}");
                        }
                        Console.WriteLine();
                    });

                    Console.WriteLine("Done.");
                    return 0;
                });
            });
        }

        public static void AddDownload(this CommandLineApplication app)
        {
            app.Command("download", (command) =>
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
                    var client = new ClientManager(new DownloadManager(downloader), new LocalRepoManager(localUri), downloader, new BinaryFormatter());

                    Console.WriteLine("Downloading repo");
                    var repoDown = client.LoadRepo(uri);
                    DumpDownloadProgress(repoDown, "Repo");
                    await repoDown;
                    foreach (var modpack in client.Modpacks)
                    {
                        Console.WriteLine($"Scheduling {modpack.Name} [{modpack.Mods.Count} mods]");
                        client.DownloadMissingFiles(modpack);
                    }
                    client.DownloadManager.DownloadStarted += (sender, d) =>
                    {
                        Console.WriteLine($"Starting {d.Source}");
                        DumpDownloadProgress(d.Progress, d.Name);
                    };
                    client.DownloadManager.CheckDownloadsToStart();
                    Console.ReadKey();
                    return 0;
                });
            });
        }

        public static void AddDump(this CommandLineApplication app)
        {
            app.Command("dump", (command) =>
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
                        var repo = (Repo)(new BinaryFormatter()).Deserialize(new FileInfo(uri.LocalPath).OpenRead());
                        repo.BaseUri = new Uri("http://base.uri/repo/");
                        DumpRepo(repo);
                    }
                    else
                    {
                        var downloader = new HttpClientDownloader();
                        var client = new ClientManager(new DownloadManager(downloader), null, downloader, new BinaryFormatter());

                        Console.WriteLine("Downloading repo");
                        var repoDown = client.LoadRepo(uri);
                        DumpDownloadProgress(repoDown, "Repo");
                        repoDown.Wait();
                        foreach (var repo in client.Repos)
                        {
                            DumpRepo(repo);
                        }
                    }

                    return 0;
                });
            });
        }

        public static void AddImport(this CommandLineApplication app)
        {
            app.Command("import", (command) =>
            {
                command.Description = "Copies every file in the folder and renames it to its hash";
                command.HelpOption("-?|-h|--help");
                var pathArg = command.Argument("[path]", "");
                var pathDestArg = command.Argument("[dest path]", "");

                command.OnExecute(async () =>
                {
                    var pathStr = pathArg.Value ?? ".";
                    var pathDestStr = pathDestArg.Value ?? "./hashed";
                    var path = Path.Combine(Directory.GetCurrentDirectory(), pathStr);
                    var pathDest = Path.Combine(Directory.GetCurrentDirectory(), pathDestStr);

                    var hashing = new Hashing(new XXHash64());

                    foreach (var file in hashing.GetFiles(new DirectoryInfo(path)))
                    {
                        if (file.Length <= 0) continue;
                        var size = file.Length.Bytes();
                        Console.Write($"{size.Humanize("#").PadLeft(10)}: ");
                        using (var mmfile = MemoryMappedFile.CreateFromFile(file.FullName, FileMode.Open, null, 0, MemoryMappedFileAccess.Read))
                        using (var src = mmfile.CreateViewStream(0, 0, MemoryMappedFileAccess.Read))
                        {
                            var start = DateTime.Now;
                            var hash = await hashing.GetFileHash(src, CancellationToken.None);
                            var filehash = new FileWithHash(file, hash);
                            var speed = size.Per((DateTime.Now - start));
                            Console.Write($"{hash} {speed.Humanize("#").PadLeft(10)}(hash) | ");
                            var fileDest = Path.Combine(pathDest, filehash.Hash.ToString());
                            try
                            {
                                if (new FileInfo(fileDest).Exists)
                                {
                                    Console.Write($"                 | {file.FullName}");
                                }
                                else
                                {
                                    using (var dest = new FileStream(fileDest, FileMode.Create, FileAccess.Write))
                                    {
                                        start = DateTime.Now;
                                        using (var src2 = mmfile.CreateViewStream(0, 0, MemoryMappedFileAccess.Read))
                                        {
                                            await src2.CopyToAsync(dest);
                                        }
                                        Console.Write($"{size.Per((DateTime.Now - start)).Humanize("#").PadLeft(10)}(copy) | {file.FullName}");
                                    }
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

        public static void AddSampleRepo(this CommandLineApplication app)
        {
            app.Command("sampleRepo", (command) =>
            {
                command.Description = "Makes each subfolder of a given folder a mod";
                command.HelpOption("-?|-h|--help");
                var pathArg = command.Argument("[path]", "Path to the folder with mods");

                command.OnExecute(async () =>
                {
                    var hashing = new Hashing(new XXHash64());

                    var pathStr = pathArg.Value ?? ".";
                    var path = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), pathStr));
                    var pathUri = new Uri(path.FullName);

                    var files = new Dictionary<HashValue, Uri>();
                    var mods = new List<ModEntry>();

                    foreach (var modFolder in path.EnumerateDirectories())
                    {
                        Console.WriteLine($"Processing {modFolder.FullName}");
                        var obs = hashing.GetFileHashes(modFolder, CancellationToken.None);
                        var mod = new Mod
                        {
                            Files = new Dictionary<Uri, HashValue>(),
                            Name = modFolder.Name,
                            Version = "1.0"
                        };
                        foreach (var lazy in obs)
                        {
                            var fileHash = await lazy.Value;
                            Console.WriteLine($"Processing {fileHash.File.FullName}");
                            mod.Files.Add(new Uri(modFolder.FullName).MakeRelativeUri(new Uri(fileHash.File.FullName)), fileHash.Hash);
                            files.Add(fileHash.Hash, pathUri.MakeRelativeUri(new Uri(fileHash.File.FullName)));
                        }
                        mods.Add(new ModEntry { Mod = mod });
                    }

                    var repo = new Repo
                    {
                        Files = files,
                        Modpacks = new List<Modpack>() { new Modpack { Mods = mods } }
                    };

                    var fileName = Path.Combine(pathUri.LocalPath, "repo.bin");
                    new BinaryFormatter().Serialize(new FileInfo(fileName).Create(), repo);
                    Console.WriteLine($"Written to {fileName}");

                    return 0;
                });
            });
        }

        public static HashValue ComputeHash(FileInfo f, IHashFunction hashf)
        {
            try
            {
                using (var stream = MemoryMappedFile.CreateFromFile(f.FullName).CreateViewStream())
                {
                    var sizeMB = f.Length / (1024L * 1024);
                    var start = DateTime.Now;
                    Console.Write($"{sizeMB.ToString().PadLeft(5)}MB: ");
                    var hash = hashf.ComputeHashAsync(stream, CancellationToken.None).GetAwaiter().GetResult();
                    var elapsed = (DateTime.Now - start).TotalSeconds;
                    var speed = (long)(f.Length / elapsed) / (1024L * 1024L);
                    Console.WriteLine($"'{hash}' @{speed.ToString().PadLeft(5)}MB/s at {f.FullName}");
                    return hash;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.Error.WriteLine(e.ToString());
                return new HashValue(new byte[] { 0 });
            }
        }

        public static void DumpDownloadProgress(IObservable<DownloadProgress> obs, string name)
        {
            obs.Sample(TimeSpan.FromMilliseconds(250))
                       .Buffer(2, 1)
                       .Subscribe(progList =>
                       {
                           var prog = new DownloadProgressCombined(progList.Last(), progList.First());
                           var dbl = prog.Current.Remaining.Bytes / prog.Speed.Size.Bytes;
                           if (Double.IsNaN(dbl)) dbl = 0;
                           var eta = prog.Speed.Interval.Multiply(dbl);
                           Console.WriteLine($"\t{prog.Current.State} {name.PadRight(23)} {prog.Current.Remaining.Humanize("#.#").PadLeft(8)} left @ {prog.Speed.Humanize("#.#").PadLeft(10)} ETA: {eta.Humanize(2)}");
                       }, ex => Console.WriteLine(ex.ToString()), () => Console.WriteLine("Done"));
        }

        public static void Main(string[] args)
        {
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
            //TODO: Add hashed folder recheck

            app.Execute(args.Length > 0 ? args : new string[] { "--help" });
        }

        private static void DumpRepo(Repo repo)
        {
            Console.WriteLine($"Repo at {repo.BaseUri}");
            Console.WriteLine($"Files:");
            foreach (var file in repo.Files)
            {
                Console.WriteLine($"\t{file.Key} at {new Uri(repo.BaseUri, file.Value)}");
            }

            Console.WriteLine($"ModPacks:");
            foreach (var modpack in repo.Modpacks)
            {
                Console.WriteLine($"\tModpack '{modpack.Name}'");
                Console.WriteLine($"\tMods:");
                foreach (var mod in modpack.Mods)
                {
                    Console.WriteLine($"\t\tMod: '{mod.Mod.Name}' [{mod.Mod.Files.Count} files]");
                }
            }
        }
    }
}