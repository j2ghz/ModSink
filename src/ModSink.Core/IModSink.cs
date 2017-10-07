using ModSink.Core.Client;
using ModSink.Core.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModSink.Core
{
    public interface IModSink
    {
        IClientService Client { get; }
        IHashFunction HashFunction { get; }
        IServerManager Server { get; }
    }
}