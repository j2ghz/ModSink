using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;

namespace ModSink.CLI.Verbs
{
    public class Chunk
    {
        public static int Run(Options opts)
        {
            var fileStream =
                new FileStream(opts.Path, FileMode.Open, FileAccess.Read, FileShare.Read, opts.FileStreamBuffer);
            var queue = new Queue<byte>(opts.Zeroes);
            var buffer = new byte[1].AsSpan();
            var lastChunkBoundary = 0L;
            var chunkSizes = new Dictionary<string, int>();
            while (fileStream.Read(buffer) != 0)
            {
                queue.Enqueue(buffer[0]);
                while (queue.Count > opts.Zeroes) queue.Dequeue();

                if (queue.ToArray().All(b => b == 0))
                {
                    var s = $"{fileStream.Position - lastChunkBoundary:G2}";
                    var length = double.Parse(s).ToString().PadLeft(10);
                    chunkSizes.TryGetValue(length, out var occurrences);
                    chunkSizes[length] = occurrences + 1;
                    while (fileStream.ReadByte() != 0)
                    {
                    }

                    lastChunkBoundary = fileStream.Position;
                }
            }

            foreach (var (key, value) in chunkSizes.OrderBy(pair => pair.Value)) Console.WriteLine($"{key}\t{value}");

            return 0;
        }

        [Verb("chunk", HelpText = "Reports chunk sizes for a specified file")]
        public class Options
        {
            [Option('b', "buffer", Default = 10 * 1024 * 1024)]
            public int FileStreamBuffer { get; set; }

            [Value(0, HelpText = "Path to file to check")]
            public string Path { get; set; }

            [Option('n', "zeroes", Default = 13, HelpText = "Amount of zeroes for chunk boundary", Min = 1,
                Max = byte.MaxValue)]
            public int Zeroes { get; set; }
        }
    }
}