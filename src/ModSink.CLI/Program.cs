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

                    hashes.Subscribe(Observer.Create<(HashValue, FileInfo)>(a => Console.WriteLine($"{a.Item1.ToString()} {a.Item2.FullName}")));

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
                        foreach (var file in obs)
                        {
                            Console.WriteLine($"Processing {file.file.FullName}");
                            mod.Files.Add(new Uri(modFolder.FullName).MakeRelativeUri(new Uri(file.file.FullName)), file.hash);
                            files.Add(file.hash, pathUri.MakeRelativeUri(new Uri(file.file.FullName)));
                        }
                        mods.Add(new ModEntry { Mod = mod });
                    }

                    var repo = new Repo
                    {
                        Files = files,
                        Modpacks = new List<Modpack>() { new Modpack { Mods = mods } }
                    };

                    var fileName = Path.GetTempFileName();
                    new BinaryFormatter().Serialize(new FileInfo(fileName).OpenWrite(), repo);
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
                catch (System.IO.DirectoryNotFoundException e)
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
                catch (System.IO.DirectoryNotFoundException e)
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
            app.Name = "ModSink.CLI";
            app.HelpOption("-?|-h|--help");

            app.AddHash();
            app.AddColCheck();
            app.AddSampleRepo();

            app.Execute(args);
        }
    }
}