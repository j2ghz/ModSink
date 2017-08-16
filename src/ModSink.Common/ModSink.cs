using ModSink.Core;
using ModSink.Core.Local;

namespace ModSink.Common
{
    public class ModSink : IModSink
    {
        public IClientManager Client { get; }
        public IHashFunction HashFunction { get; }
        public IServerManager Server { get; }
    }
}