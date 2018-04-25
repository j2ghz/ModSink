using System;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using ModSink.Core.Client;
using ReactiveUI;

namespace ModSink.WPF.Model
{
    public class SettingsModel : ReactiveObject
    {
        public SettingsModel(IClientService client)
        {
            Client = client;
            client.GroupUrls.Connect().ObserveOnDispatcher()
                .Bind(GroupUrls)
                .Subscribe();

            // Temporary to ease testing
            client.GroupUrls.Add(@"https://a3.417rct.org/Swifty_repos/modsinktestrepo/417RCT/group.bin");
        }

        public IClientService Client { get; }

        public ObservableCollectionExtended<string> GroupUrls { get; } = new ObservableCollectionExtended<string>();
    }
}