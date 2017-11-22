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
            this.Client = client;
            client.RepoUrls.Connect().ObserveOnDispatcher()
                .Bind(RepoUrls)
                .Subscribe();

            // Temporary to ease testing
            client.RepoUrls.Add(@"https://a3.417rct.org/Swifty_repos/modsinktestrepo/repo.bin");
        }

        public IClientService Client { get; }

        public ObservableCollectionExtended<string> RepoUrls { get; } = new ObservableCollectionExtended<string>();
    }
}