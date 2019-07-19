namespace ModSink.Domain.Entities.File
{
    [Equals]
    public class ChunkSignature
    {
        public ChunkSignature(Hash hash, long length)
        {
            Hash = hash;
            Length = length;
        }

        public Hash Hash { get; }
        public long Length { get; }
    }
}