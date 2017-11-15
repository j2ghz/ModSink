using System;
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
            if (!await Task.Run(() => fi.Exists)) return false;
            if (Convert.ToUInt64(fi.Length) == fileSignature.Length)
                return true;
            throw new FileSignatureException(fileSignature, new FileSignature(fileSignature.Hash, fi.Length));
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
    }
}