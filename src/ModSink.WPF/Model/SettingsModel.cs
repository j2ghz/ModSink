using DynamicData;
using DynamicData.Alias;
using DynamicData.ReactiveUI;
using ModSink.Core.Client;
using ModSink.Core.Models.Repo;
using ReactiveUI;

namespace ModSink.WPF.Model
{
    public class SettingsModel : ReactiveObject
    {
        private readonly IClientService client;

        public SettingsModel(IClientService client)
        {
            this.client = client;

            // Temporary to ease testing
            client.RepoUrls.Add(@"https://a3.417rct.org/Swifty_repos/modsinktestrepo/repo.bin");
        }
       
    }
}