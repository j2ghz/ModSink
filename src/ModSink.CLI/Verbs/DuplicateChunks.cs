using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Text;
using CommandLine;
using Humanizer;
using MessagePack;
using MessagePack.Resolvers;
using ModSink.Application.Hashing;
using ModSink.Infrastructure.Hashing;
using Newtonsoft.Json;

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

            string Report()
            {
                return $"Processed: {size.Bytes().Humanize(f)} @ {size.Bytes().Per(sw.Elapsed).Humanize(f)}\tDuplicate size: {duplicateSize.Bytes().Humanize(f)}\t";
            }

            var sizes = new Dictionary<string, long>
            {
                {"JSON (Newtonsoft)", 0L}, {"JSON (Newtonsoft) (Brotli)", 0L}, {"MessagePackSerializer", 0L},
                {"MessagePackSerializer (LZ4)", 0L}
            };


            sw.Start();
            foreach (var file in files)
            {
                size += file.Length;
                var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                var segments = new StreamBreaker(new XXHash64(), options.ChunkSize)
                    .GetSegments(stream, stream.Length)
                    .ToArray();
                foreach (var segment in segments)
                {
                    var added = hashset.Add(segment.Hash.Value);
                    if (!added)
                        duplicateSize += segment.Length;
                }

                var metadata = new Metadata(file.FullName, segments);
                var bytes = Encoding.Default
                    .GetBytes(JsonConvert.SerializeObject(metadata));
                sizes["JSON (Newtonsoft)"] += bytes.LongLength;
                var compressedStream = new MemoryStream();
                using var b = new BrotliStream(compressedStream, CompressionLevel.Fastest);
                b.Write(bytes);
                sizes["JSON (Newtonsoft) (Brotli)"] += compressedStream.Length;
                sizes["MessagePackSerializer"] += MessagePackSerializer
                    .Serialize(metadata, ContractlessStandardResolver.Instance).Length;
                sizes["MessagePackSerializer (LZ4)"] += LZ4MessagePackSerializer
                    .Serialize(metadata, ContractlessStandardResolver.Instance).Length;

                Console.WriteLine($"{Report()}\tProcessed file {file.FullName}");
            }

            sw.Stop();
            Console.WriteLine($"Processed {size.Bytes().Humanize(f)} of files in {sw.Elapsed}");


            foreach (var (key, value) in sizes) Console.WriteLine($"{key}\t{value.Bytes().Humanize(f)}");
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
                return unchecked((int)b);
            }
        }

        [Serializable]
        public class Metadata
        {
            public Metadata(string path, StreamBreaker.Segment[] segments)
            {
                Path = path;
                Segments = segments;
            }

            public string Path { get; set; }
            public StreamBreaker.Segment[] Segments { get; set; }
        }

        [Verb("dupl", HelpText = "Report duplicate chunk count, size")]
        public class Options
        {
            [Option('c', "chunkSize", Default = (byte)9, Required = false)]
            public byte ChunkSize { get; set; }

            //[Option('b', "buffer", Default = 10 * 1024 * 1024)]
            //public int FileStreamBuffer { get; set; }

            [Value(0, HelpText = "Path to file or directory to check", Required = true)]
            public string Path { get; set; }
        }
    }
}