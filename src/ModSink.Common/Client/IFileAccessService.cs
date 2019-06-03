using System.Collections.Generic;
using System.IO;
using ModSink.Common.Models.DTO.Repo;

namespace ModSink.Common.Client
{
    public interface IFileAccessService
    {
        IEnumerable<FileSignature> FilesAvailable();

        Stream Read(FileSignature fileSignature, bool temporary);

        void TemporaryFinished(FileSignature fileSignature);

        Stream Write(FileSignature fileSignature, bool temporary);
    }
}