using ModSink.Core.Models.Remote.Repo;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ModSink.Core.Models.Local
{
    internal interface IFileSystem
    {
        Task LinkFile(IHashValue file, string path);
    }
}