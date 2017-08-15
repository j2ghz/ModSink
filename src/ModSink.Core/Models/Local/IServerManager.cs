using ModSink.Core.Models.Game;
using ModSink.Core.Models.Remote.Repo;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModSink.Core.Models.Local
{
    public interface IServerManager
    {
        IEnumerable<IGameLauncher> Games { get; }

        IObservable<(IHashValue, FileInfo)> GetFileHashes(DirectoryInfo directory);

        Task<(IHashValue, FileInfo)> GetFileHash(FileInfo file);
    }
}