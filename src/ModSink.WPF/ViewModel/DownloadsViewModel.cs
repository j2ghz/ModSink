using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using ModSink.Core;
using System.Reactive.Linq;
using ModSink.Common.Client;
using ModSink.Core.Client;

namespace ModSink.WPF.ViewModel
{
    public interface IDownloadsViewModel : IRoutableViewModel
    {
        ReactiveCommand DownloadMissing { get; }
        ICollection<IDownload> Downloads { get; }
        ReactiveCommand LoadRepo { get; }
        string Url { get; }
    }

    public class DownloadsViewModel : ReactiveObject, IDownloadsViewModel
    {
        private readonly IClientManager clientManager;
        private readonly ReactiveCommand downloadMissing;
        private readonly ReactiveCommand loadRepo;
        private string url;

        public DownloadsViewModel(IScreen screen, IClientManager clientManager)
        {
            this.HostScreen = screen;
            this.clientManager = clientManager;

            var canLoadRepo = this.WhenAnyValue(x => x.Url, (url) => Uri.IsWellFormedUriString(url, UriKind.Absolute));
            this.loadRepo = ReactiveCommand.Create(async () =>
            {
                await this.clientManager.LoadRepo(new Uri(this.Url));
            }, canLoadRepo);

            //var canDownloadMissing = this.WhenAnyValue(x => x.clientManager.Repos, (repos) => repos.Any());
            this.downloadMissing = ReactiveCommand.Create(() =>
            {
                this.clientManager.Modpacks.ForEach(this.clientManager.DownloadMissingFiles);
                clientManager.DownloadManager.CheckDownloadsToStart();
            });//, canDownloadMissing);
        }

        public ReactiveCommand DownloadMissing => this.downloadMissing;
        public ICollection<IDownload> Downloads => this.clientManager.DownloadManager.Downloads;
        public IScreen HostScreen { get; protected set; }

        public ReactiveCommand LoadRepo => this.loadRepo;

        public string Url
        {
            get { return this.url; }
            set { this.RaiseAndSetIfChanged(ref this.url, value); }
        }

        public string UrlPathSegment
        {
            get { return "downloads"; }
        }
    }
}