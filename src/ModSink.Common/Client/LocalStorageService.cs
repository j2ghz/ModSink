using System;
using System.ComponentModel.Design;
using System.IO;
using System.Threading.Tasks;
using ModSink.Core.Client;
using ModSink.Core.Models.Repo;

namespace ModSink.Common.Client
{
    public class LocalStorageService : ILocalStorageService
    {
        private readonly Uri localPath;

        public LocalStorageService(Uri localPath)
        {
            this.localPath = localPath;
            var localDir = new DirectoryInfo(localPath.LocalPath);
            if (!localDir.Exists)
                localDir.Create();
        }

        public async Task Delete(FileSignature fileSignature)
        {
            var fi = await GetFileInfo(fileSignature);
            await Task.Run(() => fi.Delete());
        }

        public async Task<FileInfo> GetFileInfo(FileSignature fileSignature)
        {
            return await Task.Run(() => new FileInfo(GetFileUri(fileSignature).LocalPath));
        }

        public string GetFileName(FileSignature fileSignature)
        {
            return fileSignature.Hash.ToString();
        }

        public Uri GetFileUri(FileSignature fileSignature)
        {
            return new Uri(localPath, GetFileName(fileSignature));
        }

        public async Task<bool> IsFileAvailable(FileSignature fileSignature)
        {
            var fi = await GetFileInfo(fileSignature);
            return await Task.Run(() => fi.Exists);
        }

        public async Task<Stream> Read(FileSignature fileSignature)
        {
            var file = await GetFileInfo(fileSignature);
            return await Task.Run(() => file.Open(FileMode.Open, FileAccess.Read));
        }

        public async Task<Stream> Write(FileSignature fileSignature)
        {
            var file = await GetFileInfo(fileSignature);
            return await Task.Run(() => file.Open(FileMode.Create, FileAccess.Write));
        }

        public async Task<bool> WriteIfMissingOrInvalid(FileSignature fileSignature, out Stream stream)
        {
            var fi = await GetFileInfo(fileSignature);
            var exists = await IsFileAvailable(fileSignature);
            if (exists == false)
            {
                stream = await Write(fileSignature);
                return false;
            }
            if (fileSignature.Length == Convert.ToUInt64(fi.Length))
            {
                stream = null;
                return true;
            }
            stream = await Write(fileSignature);
            return false;
        }
    }
}