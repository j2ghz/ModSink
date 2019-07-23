using System.Collections.Generic;

namespace ModSink.Domain.Entities.File
{
    /// <summary>
    /// <see cref="FileSignature"/> with a collection of <see cref="Chunk"/>s
    /// </summary>
    public class FileChunks
    {
        public FileChunks(FileSignature file, IReadOnlyCollection<Chunk> chunks)
        {
            File = file;
            Chunks = chunks;
        }

        public IReadOnlyCollection<Chunk> Chunks { get; }

        public FileSignature File { get; }
    }
}