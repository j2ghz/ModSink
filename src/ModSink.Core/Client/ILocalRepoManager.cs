using ModSink.Core.Models.Repo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ModSink.Core.Client
{
    public interface ILocalRepoManager
    {
        Stream Create(HashValue hash);

        Uri GetFileUri(HashValue hash);

        bool IsFileAvailable(HashValue hash);

        Stream Open(HashValue hash);
    }
}