using System;
using System.Collections.Generic;
using System.Text;

namespace ModSink.Domain.Entities.File
{
    public struct Chunk
    {
        public ChunkSignature Signature { get; set; }
        public long Position { get; set; }
        public long Length => Signature.Length;
    }
}
