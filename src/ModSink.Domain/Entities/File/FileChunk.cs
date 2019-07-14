namespace ModSink.Domain.Entities.File
{
    public struct FileChunk
    {

        public FileSignature File { get; set; }
        public Chunk Chunk { get; set; }
    }
}