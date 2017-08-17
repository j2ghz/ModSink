using ModSink.Core.Local;
using ModSink.Core.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModSink.Core
{
    public interface IModSink
    {
        IClientManager Client { get; }
        IHashFunction HashFunction { get; }
        IServerManager Server { get; }
    }
}