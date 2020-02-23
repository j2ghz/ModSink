using System;
using ModSink.Domain.Entities.File;
using PathLib;

namespace ModSink.Domain.Entities.Repo
{

    /// <summary>
    /// <see cref="File.Signature"/> with <see cref="IPurePath"/> to the file
    /// </summary>
    [Equals]
    public class RelativePathFile
    {
        public Signature Signature { get; set; }
        public IPurePath RelativePath { get; set; }

        public RelativePathFile InDirectory(IPurePath dir)
        {
            return new RelativePathFile() { Signature = Signature, RelativePath = dir.Join(RelativePath) };
        }

        public static bool operator ==(RelativePathFile left, RelativePathFile right) => Operator.Weave(left, right);
        public static bool operator !=(RelativePathFile left, RelativePathFile right) => Operator.Weave(left, right);

    }
}
