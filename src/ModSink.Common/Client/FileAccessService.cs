using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ModSink.Common.Models.Repo;

namespace ModSink.Common.Client
{
    public class FileAccessService : IFileAccessService
    {
        private readonly DirectoryInfo localDir;

        public FileAccessService(DirectoryInfo localDir)
        {
            this.localDir = localDir;
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
            return await Task.Run(() => new FileInfo(GetFileUri(fileSignature).FullName));
        }

        public string GetFileName(FileSignature fileSignature)
        {
            return fileSignature.Hash.ToString();
        }

        public FileInfo GetFileUri(FileSignature fileSignature)
        {
            return localDir.ChildFile(GetFileName(fileSignature));
        }

        public async Task<bool> IsFileAvailable(FileSignature fileSignature)
        {
            var fi = await GetFileInfo(fileSignature);
            return await Task.Run(() => fi.Exists);
        }

        public async Task<Stream> Read(FileSignature fileSignature)
        {
            var file = await GetFileInfo(fileSignature);
            return await Task.Run(() => file.Open(FileMode.Open, FileAccess.Read, FileShare.Read));
        }

        public async Task<Stream> Write(FileSignature fileSignature)
        {
            var file = await GetFileInfo(fileSignature);
            return await Task.Run(() => file.Open(FileMode.Create, FileAccess.Write, FileShare.None));
        }

        public IEnumerable<FileInfo> GetFiles()
        {
            return localDir.EnumerateFiles();
        }
    }
}