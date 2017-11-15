using System;
using System.IO;
using System.Threading.Tasks;
using ModSink.Core.Client;
using ModSink.Core.Models.Repo;

namespace ModSink.Common.Client
{
    public class LocalStorageService : ILocalStorageService
    {
        private readonly DirectoryInfo localDir;
        private readonly Uri localPath;

        public LocalStorageService(Uri localPath)
        {
            this.localPath = localPath;
            localDir = new DirectoryInfo(localPath.LocalPath);
            if (!localDir.Exists)
                localDir.Create();
        }

        public async Task Delete(HashValue hash)
        {
            var fi = await GetFileInfo(hash);
            await Task.Run(() => fi.Delete());
        }

        public async Task<FileInfo> GetFileInfo(HashValue hash)
        {
            return await Task.Run(() => new FileInfo(GetFileUri(hash).LocalPath));
        }

        public string GetFileName(HashValue hash)
        {
            return hash.ToString();
        }

        public Uri GetFileUri(HashValue hash)
        {
            return new Uri(localPath, hash.ToString());
        }

        public async Task<bool> IsFileAvailable(HashValue hash)
        {
            var fi = await GetFileInfo(hash);
            return await Task.Run(() => fi.Exists);
        }

        public async Task<Stream> Read(HashValue hash)
        {
            var file = await GetFileInfo(hash);
            return await Task.Run(() => file.Open(FileMode.Open, FileAccess.Read));
        }

        public async Task<Stream> Write(HashValue hash)
        {
            var file = await GetFileInfo(hash);
            return await Task.Run(() => file.Open(FileMode.Create, FileAccess.Write));
        }
    }
}