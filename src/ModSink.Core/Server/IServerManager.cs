using ModSink.Core.Models.Repo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ModSink.Core.Server
{
    public interface IServerManager
    {
        IEnumerable<IGameLauncher> Games { get; }
    }
}