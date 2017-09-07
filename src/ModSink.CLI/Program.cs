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
                    GetFiles(path)
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
                command.Description = "Downloads a file";
                command.HelpOption("-?|-h|--help");
                var uriArg = command.Argument("[uri]", "Uri to file to download");

                command.OnExecute(() =>
                {
                    var uriStr = uriArg.Value;
                    var uri = new Uri(uriStr);

                    var client = new HttpClientDownloader();
                    var obs = client.Download(uri, new FileInfo(Path.GetTempFileName()).OpenWrite());
                    obs.Sample(TimeSpan.FromMilliseconds(100))
                       .Buffer(2, 1)
                       .Subscribe(progList =>
                       {
                           var prog = new DownloadProgressCombined(progList.Last(), progList.First());
                           Console.WriteLine($"{prog.Current.Size}b {prog.Current.Downloaded}b {prog.Speed}b/s {prog.Current.State}");
                       }, ex => Console.WriteLine(ex.ToString()), () => Console.WriteLine("Done"));

                    obs.Wait();
                    return 0;
                });
            });
        }

        public static void AddHash(this CommandLineApplication app)
        {
            app.Command("hash", (command) =>
            {
                command.Description = "Returns hash(es) of file(s)";
                command.HelpOption("-?|-h|--help");
                var pathArg = command.Argument("[path]", "Path to file to hash. If folder is provided, all files inside will be hashed");

                command.OnExecute(() =>
                {
                    var pathStr = pathArg.Value ?? ".";
                    var path = Path.Combine(Directory.GetCurrentDirectory(), pathStr);

                    var hash = new Hashing(new XXHash64());

                    var hashes = hash.GetFileHashes(new DirectoryInfo(path));

                    hashes.Subscribe(Observer.Create<FileWithHash>(a => Console.WriteLine(a.ToString())));

                    Console.WriteLine("Done.");
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
                var pathArg = command.Argument("[path]", "Path to file to hash. If folder is provided, all files inside will be hashed");

                command.OnExecute(() =>
                {
                    var pathStr = pathArg.Value ?? ".";
                    var path = Path.Combine(Directory.GetCurrentDirectory(), pathStr);

                    var hash = new Hashing(new XXHash64());

                    hash.GetFileHashes(new DirectoryInfo(path))
                    .Do(fwh =>
                    {
                        Console.Write(fwh.ToString());
                        fwh.File.OpenRead().CopyToAsync(new FileInfo(@"D:\hashed\" + fwh.Hash.ToString()).OpenWrite()).GetAwaiter().GetResult();
                        Console.WriteLine(" Done.");
                    }).Wait();

                    Console.WriteLine("Done.");
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

                command.OnExecute(() =>
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
                        var obs = hashing.GetFileHashes(modFolder).ToEnumerable();
                        var mod = new Mod
                        {
                            Files = new Dictionary<Uri, HashValue>(),
                            Name = modFolder.Name,
                            Version = "1.0"
                        };
                        foreach (var fileHash in obs)
                        {
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

        public static IEnumerable<FileInfo> GetFiles(string root)
        {
            var dirs = new Stack<string>();

            if (!System.IO.Directory.Exists(root))
            {
                throw new ArgumentException();
            }
            dirs.Push(root);

            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                try
                {
                    System.IO.Directory.GetDirectories(currentDir).ForEach(dirs.Push);
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
                catch (DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }

                string[] files;
                try
                {
                    files = System.IO.Directory.GetFiles(currentDir);
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
                catch (DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
                foreach (var f in files.Select(f => new FileInfo(f)))
                {
                    yield return f;
                }
            }
        }

        public static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.Name = "modsink";
            app.FullName = "ModSink.CLI";
            app.HelpOption("-?|-h|--help");
            app.ShortVersionGetter = () => typeof(Program).Assembly.GetName().Version.ToString();

            app.AddHash();
            app.AddColCheck();
            app.AddSampleRepo();
            app.AddDownload();
            app.AddImport();

            app.Execute(args.Length > 0 ? args : new string[] { "--help" });
        }
    }
}