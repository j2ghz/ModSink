using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Security.Cryptography;
using CommandLine;
using Humanizer;
using ModSink.Application.Hashing;

namespace ModSink.CLI.Verbs
{
    public class DuplicateChunks : BaseVerb<DuplicateChunks.Options>
    {
        public override int Run(Options options)
        {
            var files = new List<IFileInfo>();
            var fileSystem = new FileSystem();
            if (fileSystem.File.Exists(options.Path))
                files.Add(fileSystem.FileInfo.FromFileName(options.Path));
            else if (fileSystem.Directory.Exists(options.Path))
                files.AddRange(GetFiles(fileSystem.DirectoryInfo.FromDirectoryName(options.Path)));

            var hashset = new HashSet<byte[]>();
            var duplicateSize = 0L;
            foreach (var file in files)
            {
                var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                var segments = new StreamBreaker().GetSegments(stream, stream.Length, xxHash64.Create());
                foreach (var segment in segments)
                {
                    var added = hashset.Add(segment.Hash);
                    if (!added)
                    {
                        duplicateSize += segment.Length;
                        Console.WriteLine(
                            $"{duplicateSize.Bytes().Humanize("G03")}\tDuplicate {BitConverter.ToString(segment.Hash)}\tin {file.FullName}");
                    }
                }

                Console.WriteLine($"{duplicateSize.Bytes().Humanize("G03")}\tProcessed file {file.FullName}");
            }

            Console.WriteLine(
                $"Unique chunks: {hashset.Count}\tDuplicate size: {duplicateSize.Bytes().Humanize("G03")}");
            return 0;
        }

        private static IEnumerable<IFileInfo> GetFiles(IDirectoryInfo directory)
        {
            var directoryStack = new Stack<IDirectoryInfo>();
            directoryStack.Push(directory);

            while (directoryStack.Count > 0)
            {
                var dir = directoryStack.Pop();
                foreach (var d in dir.EnumerateDirectories()) directoryStack.Push(d);
                foreach (var file in dir.EnumerateFiles()) yield return file;
            }
        }

        [Verb("dupl", HelpText = "Report duplicate chunk count, size")]
        public class Options
        {
            [Option('b', "buffer", Default = 10 * 1024 * 1024)]
            public int FileStreamBuffer { get; set; }

            [Value(0, HelpText = "Path to file or directory to check", Required = true)]
            public string Path { get; set; }
        }
    }
}