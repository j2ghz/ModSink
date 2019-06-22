using ModSink.Domain.Entities.File;

namespace ModSink.Domain.Entities.Repo
{
    public class RelativeUriFile
    {
        public FileSignature Signature { get; set; }
        public RelativeUri RelativeUri { get; set; }
    }
}