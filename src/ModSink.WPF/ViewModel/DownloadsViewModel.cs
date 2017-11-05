using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Aggregation;
using DynamicData.ReactiveUI;
using Humanizer;
using ModSink.Core.Client;
using ReactiveUI;
using Serilog;

namespace ModSink.WPF.ViewModel
{
    public class DownloadsViewModel : ReactiveObject
    {
        private readonly ReactiveList<DownloadViewModel> downloads = new ReactiveList<DownloadViewModel>();
        private readonly ILogger log;
        private readonly ObservableAsPropertyHelper<string> queueCount;
        private string url = @"https://a3.417rct.org/Swifty_repos/modsinktestrepo/repo.bin";

        public DownloadsViewModel(IClientService clientService, ILogger log)
        {
            this.log = log;
            DownloadMissing = ReactiveCommand.CreateFromTask(async () =>
            {
                await clientService.DownloadMissingFiles(clientService.Modpacks.Items.First());
            });
            LoadRepo = ReactiveCommand.CreateFromObservable(() => clientService.LoadRepo(new Uri(Url)));
            var ds = clientService.DownloadService.Downloads.Connect();
            ds
                .FilterOnProperty(d => d.State, d => d.State == DownloadState.Downloading)
                .Transform(d => new DownloadViewModel(d))
                .Bind(downloads)
                .Subscribe();
            queueCount = ds
                .FilterOnProperty(d => d.State, d => d.State == DownloadState.Queued)
                .Count()
                .Select(i => "file".ToQuantity(i))
                .ToProperty(this, t => t.QueueCount);
        }

        public ReactiveCommand DownloadMissing { get; }
        public IReadOnlyReactiveList<DownloadViewModel> Downloads => downloads;
        public string QueueCount => queueCount.Value;
        public ReactiveCommand<Unit, DownloadProgress> LoadRepo { get; }

        public string Url
        {
            get => url;
            set => this.RaiseAndSetIfChanged(ref url, value);
        }
    }
}