using System;
using System.IO;
using System.Threading.Tasks;
using ModSink.Common.Models.Repo;

namespace ModSink.Common.Client
{
    public interface IFileAccessService
    {
        Task Delete(FileSignature fileSignature);

        Task<FileInfo> GetFileInfo(FileSignature fileSignature);

        string GetFileName(FileSignature fileSignature);

        FileInfo GetFileUri(FileSignature fileSignature);

        Task<bool> IsFileAvailable(FileSignature fileSignature);

        Task<Stream> Read(FileSignature fileSignature);

        Task<Stream> Write(FileSignature fileSignature);
        
    }
}