using ModSink.Core.Models.Local;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModSink.Core.Models
{
    public interface IModSink
    {
        IClientManager Client { get; }
        IHashFunction HashFunction { get; }
        IServerManager Server { get; }
    }
}