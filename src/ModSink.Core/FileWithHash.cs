using ModSink.Core.Models.Repo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ModSink.Core
{
    public class FileWithHash
    {
        public FileWithHash(FileInfo file, HashValue hash)
        {
            this.File = file;
            this.Hash = hash;
        }

        public FileInfo File { get; set; }
        public HashValue Hash { get; set; }

        public override string ToString()
        {
            return $"{Hash} {File.FullName}";
        }
    }
}