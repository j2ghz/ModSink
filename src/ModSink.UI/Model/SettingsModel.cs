using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using ModSink.Common.Client;
using ReactiveUI;

namespace ModSink.UI.Model
{
    public class SettingsModel : ReactiveObject
    {
        public SettingsModel(ClientService client)
        {
            Client = client;
            client.GroupUrls.Connect().ObserveOn(RxApp.MainThreadScheduler)
                .Bind(GroupUrls)
                .Subscribe();

            // Temporary to ease testing
            RxApp.TaskpoolScheduler.Schedule(() =>
                client.GroupUrls.AddOrUpdate(@"https://files.417rct.org/Swifty_repos/modsink/group.bin"));
        }

        public ClientService Client { get; }

        public ObservableCollectionExtended<string> GroupUrls { get; } = new ObservableCollectionExtended<string>();
    }
}