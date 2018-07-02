using System;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using ModSink.Common.Client;
using ReactiveUI;

namespace ModSink.WPF.Model
{
    public class SettingsModel : ReactiveObject
    {
        public SettingsModel(ClientService client)
        {
            Client = client;
            client.GroupUrls.Connect().ObserveOnDispatcher()
                .Bind(GroupUrls)
                .Subscribe();

            // Temporary to ease testing
            client.GroupUrls.Add(@"https://modsink.j2ghz.com/group.bin");
        }

        public ClientService Client { get; }

        public ObservableCollectionExtended<string> GroupUrls { get; } = new ObservableCollectionExtended<string>();
    }
}