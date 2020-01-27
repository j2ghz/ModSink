using System.Collections.Generic;

namespace ModSink.Domain.Entities.File
{
/// <summary>
/// <see cref="Signature"/> with a collection of <see cref="Chunk"/>s
/// </summary>
public class FileChunks
{
    public FileChunks(Signature file, IReadOnlyCollection<Chunk> chunks)
    {
        File = file;
        Chunks = chunks;
    }

    public IReadOnlyCollection<Chunk> Chunks {
        get;
    }

    public Signature File {
        get;
    }
}
}
