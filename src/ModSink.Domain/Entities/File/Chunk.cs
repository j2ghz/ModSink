namespace ModSink.Domain.Entities.File
{
    public struct Chunk
    {
        public Signature Signature { get; set; }
        public long Position { get; set; }
        public long Length => Signature.Length;
    }
}
