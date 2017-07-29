using Microsoft.Extensions.CommandLineUtils;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using ModSink.Common;
using ModSink.Core.Models.Local;
using System.Threading;

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

                    if (!Directory.Exists(path))
                    {
                        throw new ArgumentException($"Can't find {path}");
                    }
                    Console.WriteLine($"Crawling {path}");
                    IHashFunction<ByteHashValue> xxhash = new XXHash64();
                    new DirectoryInfo(path).GetFiles("*", SearchOption.AllDirectories)
                    .Select(f =>
                    {
                        using (var stream = f.OpenRead())
                        {
                            var hash = xxhash.ComputeHashAsync(stream, CancellationToken.None).GetAwaiter().GetResult();
                            Console.WriteLine($"{(f.Length / (1024L * 1024)).ToString().PadLeft(4)}MB: '{hash}' at {f.FullName}");
                            return new { f, hash };
                        }
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

                    var files = new List<FileInfo>();
                    if (File.Exists(path))
                    {
                        files.Add(new FileInfo(path));
                    }
                    else if (Directory.Exists(path))
                    {
                        Console.WriteLine($"Crawling {path}");
                        files.AddRange(new DirectoryInfo(path).GetFiles("*", SearchOption.AllDirectories));
                    }
                    else
                    {
                        throw new ArgumentException($"Can't find {path}");
                    }

                    IHashFunction<ByteHashValue> hash = new Common.XXHash64();
                    foreach (var f in files)
                    {
                        using (var stream = f.OpenRead())
                        {
                            Console.Write($"{(f.Length / (1024L * 1024)).ToString().PadLeft(4)}MB: ");
                            Console.WriteLine($"'{hash.ComputeHashAsync(stream, CancellationToken.None).GetAwaiter().GetResult()}' at {f.FullName}");
                        }
                    }
                    Console.WriteLine("Done.");
                    return 0;
                });
            });
        }

        public static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.Name = "ModSink.CLI";
            app.HelpOption("-?|-h|--help");

            app.AddHash();
            app.AddColCheck();

            app.Execute(args);
        }
    }
}