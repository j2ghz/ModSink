using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using CommandLine;
using Humanizer;
using ModSink.Application.Hashing;
using ModSink.Infrastructure.Hashing;
using Newtonsoft.Json;
using PathLib;

namespace ModSink.CLI.Verbs
{
    
    public class DuplicateChunks : BaseVerb<DuplicateChunks.Options>
    {
        private const string f = "G04";
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

        public override int Run(Options options)
        {
            var sw = new Stopwatch();
            var files = new List<IFileInfo>();
            var fileSystem = new FileSystem();
            if (fileSystem.File.Exists(options.Path))
                files.Add(fileSystem.FileInfo.FromFileName(options.Path));
            else if (fileSystem.Directory.Exists(options.Path))
                files.AddRange(GetFiles(fileSystem.DirectoryInfo.FromDirectoryName(options.Path)));

            var hashset = new HashSet<byte[]>(new ByteArrayComparer());
            var size = 0L;
            var duplicateSize = 0L;
            var metadata = new Metadata();

            Func<string> report = () =>
                $"Unique chunks: {hashset.Count}\tDuplicate size: {duplicateSize.Bytes().Humanize(f)}\t";

            sw.Start();
            foreach (var file in files)
            {
                size += file.Length;
                var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                var segments = new StreamBreaker(new XXHash64(), options.ChunkSize).GetSegments(stream, stream.Length)
                    .ToArray();
                metadata.FileSegments.Add(new PurePathFactory().Create(file.FullName), segments);
                foreach (var segment in segments)
                {
                    var added = hashset.Add(segment.Hash.Value);
                    if (!added)
                        duplicateSize += segment.Length;
                }

                Console.WriteLine(
                    $"{report()}Processed file {file.FullName}");
            }
            sw.Stop();
            Console.WriteLine($"Processed {size.Bytes().Humanize(f)} of files in {sw.Elapsed}");

            sw.Restart();
            var bytes = Encoding.Default.GetBytes(JsonConvert.SerializeObject(metadata));
            sw.Stop();
            var json2size = bytes.Length.Bytes();
            Console.WriteLine($"JSON (Newtonsoft): {json2size.Humanize(f)} in {sw.Elapsed}");

            for (int i = 1; i < 12; i++)
            {
                var compressedStream = new MemoryStream();
                sw.Restart();
                using var b = new BrotliStream(compressedStream, (CompressionLevel)i);
                b.Write(bytes);
                sw.Stop();
                Console.WriteLine($"JSON (Newtonsoft) (Brotli {i}): {compressedStream.Length.Bytes().Humanize(f)} in {sw.Elapsed}");
            }

            return 0;
        }

        public class ByteArrayComparer : IEqualityComparer<byte[]>
        {
            public bool Equals(byte[] a, byte[] b)
            {
                if (a.Length != b.Length) return false;
                for (var i = 0; i < a.Length; i++)
                    if (a[i] != b[i])
                        return false;
                return true;
            }

            public int GetHashCode(byte[] a)
            {
                uint b = 0;
                for (var i = 0; i < a.Length; i++)
                    b = ((b << 23) | (b >> 9)) ^ a[i];
                return unchecked((int) b);
            }
        }
        [Serializable]
        public class Metadata
        {
            public readonly IDictionary<IPurePath, StreamBreaker.Segment[]> FileSegments = new Dictionary<IPurePath, StreamBreaker.Segment[]>();
        }

        [Verb("dupl", HelpText = "Report duplicate chunk count, size")]
        public class Options
        {
            [Option('c', "chunkSize", Default = (byte)9,Required = false)]
            public byte ChunkSize { get; set; }

            //[Option('b', "buffer", Default = 10 * 1024 * 1024)]
            //public int FileStreamBuffer { get; set; }

            [Value(0, HelpText = "Path to file or directory to check", Required = true)]
            public string Path { get; set; }
        }
    }
}