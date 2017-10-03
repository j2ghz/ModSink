using ModSink.Core.Client;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModSink.WPF.ViewModel
{
    public class DownloadsViewModel : ReactiveObject
    {
        private string url;

        public DownloadsViewModel(IClientManager clientManager)
        {
            this.ClientManager = clientManager;
            this.DownloadMissing = ReactiveCommand.Create(() =>
            {
                clientManager.DownloadMissingFiles(clientManager.Modpacks.First());
                clientManager.DownloadManager.CheckDownloadsToStart();
            });
            this.LoadRepo = ReactiveCommand.Create(() =>
            {
                var obs = clientManager.LoadRepo(new Uri(this.Url));
                obs.Subscribe(prog => Console.WriteLine(prog.State), () => Console.WriteLine("Done"));
            });
        }

        public IClientManager ClientManager { get; }

        public ReactiveCommand DownloadMissing { get; }
        public ReactiveCommand LoadRepo { get; }

        public string Url
        {
            get { return this.url; }
            set { this.RaiseAndSetIfChanged(ref this.url, value); }
        }
    }
}