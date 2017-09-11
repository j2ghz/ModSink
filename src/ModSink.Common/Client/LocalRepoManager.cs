using ModSink.Core.Client;
using System;
using System.Collections.Generic;
using System.Text;
using ModSink.Core.Models.Repo;
using System.IO;

namespace ModSink.Common.Client
{
    public class LocalRepoManager : ILocalRepoManager
    {
        private readonly DirectoryInfo localDir;
        private readonly Uri localPath;

        public LocalRepoManager(Uri localPath)
        {
            this.localPath = localPath;
            this.localDir = new DirectoryInfo(localPath.LocalPath);
        }

        public Uri GetFileUri(HashValue hash)
        {
            return new Uri(this.localPath, hash.ToString());
        }

        public bool IsFileAvailable(HashValue hash)
        {
            return new FileInfo(GetFileUri(hash).LocalPath).Exists;
        }

        public Stream Read(HashValue hash)
        {
            var uri = GetFileUri(hash);
            var file = new FileInfo(uri.LocalPath);
            return file.Open(FileMode.Open, FileAccess.Read);
        }

        public Stream Write(HashValue hash)
        {
            var uri = GetFileUri(hash);
            var file = new FileInfo(uri.LocalPath);
            return file.Open(FileMode.Create, FileAccess.Write);
        }
    }
}