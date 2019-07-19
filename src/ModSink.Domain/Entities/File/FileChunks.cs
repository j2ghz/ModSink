using System.Collections.Generic;

namespace ModSink.Domain.Entities.File
{
    public class FileChunks
    {
        public FileChunks(FileSignature file, IReadOnlyCollection<Chunk> chunks)
        {
            File = file;
            Chunks = chunks;
        }

        public IReadOnlyCollection<Chunk> Chunks { get; };

        public FileSignature File { get; }
    }
}