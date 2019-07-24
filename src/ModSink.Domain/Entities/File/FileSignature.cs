using System;
using System.IO;

namespace ModSink.Domain.Entities.File
{
    [Equals]
    public class FileSignature
    {
        public FileSignature(Hash hash, long length)
        {
            Hash = hash;
            Length = length;
        }

        public Hash Hash { get; }

        /// <summary>
        ///     The length of the file taken from <see cref="FileInfo" />, in bytes.
        /// </summary>
        [Obsolete]
        public long Length { get; }
    }
}