using ModSink.Core.Game;
using ModSink.Core.Models.Repo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ModSink.Core.Local
{
    public interface IServerManager
    {
        IEnumerable<IGameLauncher> Games { get; }

        Task<(IHashValue, FileInfo)> GetFileHash(FileInfo file);

        IObservable<(IHashValue, FileInfo)> GetFileHashes(DirectoryInfo directory);
    }
}