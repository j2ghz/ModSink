namespace ModSink.Domain.Entities.File
{
    [Equals]
    public struct Chunk
    {
        public Signature Signature { get; set; }
        public long Position { get; set; }
        public long Length => Signature.Length;

        public static bool operator ==(Chunk left, Chunk right) => Operator.Weave(left, right);
        public static bool operator !=(Chunk left, Chunk right) => Operator.Weave(left, right);

    }
}
