using ModSink.Core.Models.Repo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ModSink.Core.Client
{
    public interface ILocalRepoManager
    {
        void Delete(HashValue hash);

        Uri GetFileUri(HashValue hash);

        bool IsFileAvailable(HashValue hash);

        Stream Read(HashValue hash);

        Stream Write(HashValue hash);
    }
}