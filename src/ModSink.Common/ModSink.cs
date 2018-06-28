using System;
using ModSink.Core;
using ModSink.Core.Server;
using ModSink.Core.Client;

namespace ModSink.Common
{
    public class ModSink : IModSink
    {
        public ModSink( IClientService client)
        {
            this.Client = client;
        }

        public IClientService Client { get; }
    }
}