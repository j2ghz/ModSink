using System.IO;
using ModSink.Common.Models.DTO.Repo;

namespace ModSink.Common
{
    public class FileWithHash
    {
        public FileWithHash(FileInfo file, HashValue hash)
        {
            File = file;
            Hash = hash;
        }

        public FileInfo File { get; set; }
        public HashValue Hash { get; set; }

        public override string ToString()
        {
            return $"{Hash} {File.FullName}";
        }
    }
}