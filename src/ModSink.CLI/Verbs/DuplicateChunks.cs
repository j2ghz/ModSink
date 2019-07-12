using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using CommandLine;

namespace ModSink.CLI.Verbs
{
    public class DuplicateChunks : BaseVerb<DuplicateChunks.Options>
    {
        public override int Run(Options options)
        {
            var files = new List<IFileInfo>();
            if (File.Exists(options.Path))
                files.Add(new FileSystem().FileInfo.FromFileName(options.Path));
            else if (Directory.Exists(options.Path))
                files.AddRange(GetFiles(new FileSystem().DirectoryInfo.FromDirectoryName(options.Path)));

            //Get chunks from files
            //  add chunks to hashset
            //  count duplicate chunks + sizes
            //  print statistics


            throw new NotImplementedException();
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