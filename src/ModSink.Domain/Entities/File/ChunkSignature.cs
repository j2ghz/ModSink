using System;
using System.Collections.Generic;
using System.Text;

namespace ModSink.Domain.Entities.File
{
    public struct ChunkSignature
    {
        public Hash Hash { get; set; }
        public long Length { get; set; }
    }
}
