using System;
using ModSink.Core;
using ModSink.Core.Server;
using ModSink.Core.Client;

namespace ModSink.Common
{
    public class ModSink : IModSink
    {
        public ModSink(IHashFunction hashFunction, IClientService client, IServerManager server)
        {
            this.HashFunction = hashFunction;
            this.Client = client;
            this.Server = server;
        }

        public IClientService Client { get; }
        public IHashFunction HashFunction { get; }
        public IServerManager Server { get; }
    }
}