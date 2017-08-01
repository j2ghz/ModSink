using Microsoft.Extensions.CommandLineUtils;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using ModSink.Common;
using ModSink.Core.Models.Local;
using System.Threading;
using System.Diagnostics;

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
                    IHashFunction<ByteHashValue> xxhash = new XXHash64();
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

                    IHashFunction<ByteHashValue> hash = new Common.XXHash64();
                    foreach (var f in GetFiles(path))
                    {
                        ComputeHash(f, hash);
                    }
                    Console.WriteLine("Done.");
                    return 0;
                });
            });
        }

        public static ByteHashValue ComputeHash(FileInfo f, IHashFunction<ByteHashValue> hashf)
        {
            using (var stream = f.OpenRead())
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

            app.Execute(args);
        }
    }
}