using System;
using System.IO;
using System.Threading.Tasks;
using ModSink.Core.Models.Repo;

namespace ModSink.Core.Client
{
    public interface ILocalStorageService
    {
        Task Delete(FileSignature fileSignature);

        Task<FileInfo> GetFileInfo(FileSignature fileSignature);

        string GetFileName(FileSignature fileSignature);

        Uri GetFileUri(FileSignature fileSignature);

        Task<bool> IsFileAvailable(FileSignature fileSignature);

        Task<Stream> Read(FileSignature fileSignature);

        Task<Stream> Write(FileSignature fileSignature);
    }
    public class FileSignatureException : Exception
    {
        public FileSignatureException(FileSignature expected, FileSignature actual) : base(
            $"File with a signature {expected} was expected, but {actual} was found")
        {
        }
    }
}