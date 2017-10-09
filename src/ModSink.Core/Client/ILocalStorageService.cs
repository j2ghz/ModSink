using ModSink.Core.Models.Repo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ModSink.Core.Client
{
    public interface ILocalStorageService
    {
        Task Delete(HashValue hash);

        Task<FileInfo> GetFileInfo(HashValue hash);

        string GetFileName(HashValue hash);

        Uri GetFileUri(HashValue hash);

        Task<bool> IsFileAvailable(HashValue hash);

        Task<Stream> Read(HashValue hash);

        Task<Stream> Write(HashValue hash);
    }
}