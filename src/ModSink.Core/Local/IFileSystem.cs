using ModSink.Core.Models.Repo;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ModSink.Core.Local
{
    internal interface IFileSystem
    {
        Task LinkFile(IHashValue file, string path);
    }
}