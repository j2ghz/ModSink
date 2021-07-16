using ModSink.Domain.Entities.File;
using PathLib;

namespace ModSink.Domain.Entities.Repo
{
    /// <summary>
    ///     <see cref="File.Signature" /> with <see cref="IPurePath" /> to the file
    /// </summary>
    public class RelativePathFile
    {
        public IPurePath RelativePath { get; set; }
        public Signature Signature { get; set; }

        public RelativePathFile InDirectory(IPurePath dir) =>
            new RelativePathFile {Signature = Signature, RelativePath = dir.Join(RelativePath)};
    }
}
