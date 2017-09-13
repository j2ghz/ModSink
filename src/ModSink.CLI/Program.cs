using Microsoft.Extensions.CommandLineUtils;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Reactive;
using ModSink.Common;
using System.Threading;
using System.Diagnostics;
using ModSink.Core;
using ModSink.Core.Models.Repo;
using System.IO.MemoryMappedFiles;
using System.Reactive.Linq;
using System.Collections.Concurrent;
using System.Runtime.Serialization.Formatters.Binary;
using ModSink.Core.Client;
using ModSink.Common.Client;

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

                command.OnExecute(() =>
                {
                    var uriStr = uriArg.Value;
                    var uri = new Uri(uriStr);
                    var localStr = pathArg.Value;
                    var localUri = new Uri(localStr);
                    var downloader = new HttpClientDownloader();
                    var client = new ClientManager(new DownloadManager(downloader), new LocalRepoManager(localUri), downloader, new BinaryFormatter());

                    Console.WriteLine("Downloading repo");
                    var repoDown = client.LoadRepo(uri);
                    DumpDownloadProgress(repoDown);
                    repoDown.Wait();
                    foreach (var modpack in client.Modpacks)
                    {
                        Console.WriteLine($"Scheduling {modpack.Name} [{modpack.Mods.Count} mods]");
                        client.DownloadMissingFiles(modpack);
                    }
                    client.DownloadManager.DownloadStarted += (sender, d) => DumpDownloadProgress(d.Progress);
                    foreach (var d in client.DownloadManager.Downloads)
                    {
                        Console.WriteLine($"{d.Source} {d.State}");
                    }
                    client.DownloadManager.CheckDownloadsToStart();

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
                    var downloader = new HttpClientDownloader();
                    var client = new ClientManager(new DownloadManager(downloader), null, downloader, new BinaryFormatter());

                    Console.WriteLine("Downloading repo");
                    var repoDown = client.LoadRepo(uri);
                    DumpDownloadProgress(repoDown);
                    repoDown.Wait();
                    foreach (var repo in client.Repos)
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
                        Console.Write($"{(file.Length / (1024 * 1024)).ToString().PadLeft(5)}MB: ");
                        using (var mmfile = MemoryMappedFile.CreateFromFile(file.FullName, FileMode.Open, null, 0, MemoryMappedFileAccess.Read))
                        using (var src = mmfile.CreateViewStream(0, 0, MemoryMappedFileAccess.Read))
                        {
                            var start = DateTime.Now;
                            var hash = await hashing.GetFileHash(src, CancellationToken.None);
                            var filehash = new FileWithHash(file, hash);
                            var elapsed = (DateTime.Now - start).TotalSeconds;
                            var speed = (ulong)(file.Length / elapsed) / (1024UL * 1024UL);
                            Console.Write($"{hash} {speed.ToString().PadLeft(5)}MB/s(hash) | ");
                            var fileDest = Path.Combine(pathDest, filehash.Hash.ToString());
                            try
                            {
                                if (new FileInfo(fileDest).Exists)
                                {
                                    Console.Write($"    File exists | {file.FullName}");
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
                                        Console.Write($"{((ulong)(file.Length / (DateTime.Now - start).TotalSeconds) / (1024UL * 1024UL)).ToString().PadLeft(5)}MB/s(copy) | {file.FullName}");
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
                    var speed = (ulong)(f.Length / elapsed) / (1024UL * 1024UL);
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

        public static void DumpDownloadProgress(IObservable<DownloadProgress> obs)
        {
            obs.Sample(TimeSpan.FromMilliseconds(100))
                       .Buffer(2, 1)
                       .Subscribe(progList =>
                       {
                           var prog = new DownloadProgressCombined(progList.Last(), progList.First());
                           Console.WriteLine($"{prog.Current.Size}b {prog.Current.Downloaded}b {prog.Speed}b/s {prog.Current.State}");
                       }, ex => Console.WriteLine(ex.ToString()), () => Console.WriteLine("Done"));
        }

        //public static void AddHash(this CommandLineApplication app)
        //{
        //    app.Command("hash", (command) =>
        //    {
        //        command.Description = "Returns hash(es) of file(s)";
        //        command.HelpOption("-?|-h|--help");
        //        var pathArg = command.Argument("[path]", "Path to file to hash. If folder is provided, all files inside will be hashed");

        //        command.OnExecute(() =>
        //        {
        //            var pathStr = pathArg.Value ?? ".";
        //            var path = Path.Combine(Directory.GetCurrentDirectory(), pathStr);

        //            var hash = new Hashing(new XXHash64());

        //            var hashes = hash.GetFileHashes(new DirectoryInfo(path));
        public static void Main(string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = "modsink",
                FullName = "ModSink.CLI"
            };
            app.HelpOption("-?|-h|--help");
            app.ShortVersionGetter = () => typeof(Program).Assembly.GetName().Version.ToString();

            //app.AddHash();
            app.AddColCheck();
            app.AddSampleRepo();
            app.AddDownload();
            app.AddImport();
            app.AddDump();

            app.Execute(args.Length > 0 ? args : new string[] { "--help" });
        }
    }
}