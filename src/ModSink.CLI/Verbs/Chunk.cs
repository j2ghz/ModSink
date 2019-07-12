using System;
using System.IO;
using System.Security.Cryptography;
using CommandLine;
using Humanizer;
using ModSink.Application.Hashing;

namespace ModSink.CLI.Verbs
{
    public class Chunk
    {
        public static int Run(Options opts)
        {
            var fileStream =
                new FileStream(opts.Path, FileMode.Open, FileAccess.Read, FileShare.Read, opts.FileStreamBuffer);
            var segments = new StreamBreaker().GetSegments(fileStream, fileStream.Length, xxHash64.Create());
            foreach (var segment in segments)
                Console.WriteLine(
                    $"{BitConverter.ToString(segment.Hash)}\t{segment.Offset.Bytes().Humanize("G03")}\t{segment.Length.Bytes().Humanize("G03")}");

            return 0;
        }

        [Verb("chunk", HelpText = "Reports chunk sizes for a specified file")]
        public class Options
        {
            [Option('b', "buffer", Default = 10 * 1024 * 1024)]
            public int FileStreamBuffer { get; set; }

            [Value(0, HelpText = "Path to file to check", Required = true)]
            public string Path { get; set; }
        }
    }
}