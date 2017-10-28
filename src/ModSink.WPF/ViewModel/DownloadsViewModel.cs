using ModSink.Core.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using DynamicData.PLinq;
using DynamicData.ReactiveUI;
using DynamicData.Alias;
using Serilog;
using System.Reactive;
using DynamicData.Aggregation;
using ReactiveUI;

namespace ModSink.WPF.ViewModel
{
    public class DownloadsViewModel : ReactiveObject
    {
        private readonly ReactiveList<DownloadViewModel> downloads = new ReactiveList<DownloadViewModel>();
        private readonly ILogger log;
        private string url;

        public DownloadsViewModel(IClientService clientService, ILogger log)
        {
            this.ClientManager = clientService;
            this.log = log;
            this.DownloadMissing = ReactiveCommand.CreateFromTask(async () =>
            {
                await clientService.DownloadMissingFiles(clientService.Modpacks.Items.First());
            });
            this.LoadRepo = ReactiveCommand.CreateFromObservable(() => clientService.LoadRepo(new Uri(this.Url)));
            this.ClientManager.DownloadService.Downloads
                .Connect()
                .FilterOnProperty(d=> d.State,d=>d.State == DownloadState.Downloading)
                .Transform(d => new DownloadViewModel(d))
                .Bind(downloads)
                .Subscribe();
        }

        public IClientService ClientManager { get; }
        public ReactiveCommand DownloadMissing { get; }
        public IReadOnlyReactiveList<DownloadViewModel> Downloads => downloads;
        public ReactiveCommand<Unit, DownloadProgress> LoadRepo { get; }

        public string Url
        {
            get { return this.url; }
            set { this.RaiseAndSetIfChanged(ref this.url, value); }
        }
    }
}