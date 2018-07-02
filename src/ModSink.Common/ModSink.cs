using ModSink.Common.Client;

namespace ModSink.Common
{
    public class ModSink
    {
        public ModSink(ClientService client)
        {
            Client = client;
        }

        public ClientService Client { get; }
    }
}