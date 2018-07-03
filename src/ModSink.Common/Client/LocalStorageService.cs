using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DynamicData;
using ModSink.Common.Models.Repo;

namespace ModSink.Common.Client
{
    public class LocalStorageService : ILocalStorageService
    {
        private readonly SourceList<FileSignature> filesAvailable = new SourceList<FileSignature>();
        private readonly DirectoryInfo localDir;

        public LocalStorageService(DirectoryInfo localDir)
        {
            this.localDir = localDir;
            if (!localDir.Exists)
                localDir.Create();
            filesAvailable.Edit(l =>
            {
                l.AddRange(localDir.EnumerateFiles()
                    .Select(fi => new FileSignature(new HashValue(fi.Name), fi.Length)));
            });
        }

        public IObservableList<FileSignature> FilesAvailable => filesAvailable.AsObservableList();

        public async Task Delete(FileSignature fileSignature)
        {
            var fi = await GetFileInfo(fileSignature);
            filesAvailable.Remove(fileSignature);
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
        [Obsolete]
        public async Task<bool> IsFileAvailable(FileSignature fileSignature)
        {
            var fi = await GetFileInfo(fileSignature);
            return await Task.Run(() => fi.Exists);
        }

        public async Task<Stream> Read(FileSignature fileSignature)
        {
            var file = await GetFileInfo(fileSignature);
            return await Task.Run(() => file.Open(FileMode.Open, FileAccess.Read,FileShare.Read));
        }

        public async Task<Stream> Write(FileSignature fileSignature)
        {
            var file = await GetFileInfo(fileSignature);
            return await Task.Run(() => file.Open(FileMode.Create, FileAccess.Write,FileShare.None));
        }

        public async Task<(bool available, Lazy<Task<Stream>> stream)> WriteIfMissingOrInvalid(
            FileSignature fileSignature)
        {
            var fi = await GetFileInfo(fileSignature);
            var exists = await IsFileAvailable(fileSignature);
            if (exists == false || fileSignature.Length != Convert.ToUInt64(fi.Length))
                return (false, new Lazy<Task<Stream>>(() => Write(fileSignature)));
            return (true, null);
        }
    }
}