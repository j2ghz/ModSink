using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Aggregation;
using DynamicData.ReactiveUI;
using Humanizer;
using ModSink.Core.Client;
using ReactiveUI;

namespace ModSink.WPF.ViewModel
{
    public class DownloadsViewModel : ReactiveObject
    {
        private readonly CompositeDisposable disposable = new CompositeDisposable();
        private readonly ReactiveList<DownloadViewModel> downloads = new ReactiveList<DownloadViewModel>();
        private readonly ObservableAsPropertyHelper<string> queueCount;

        public DownloadsViewModel(IClientService clientService)
        {
            DownloadMissing = ReactiveCommand.CreateFromTask(async () =>
            {
                await clientService.DownloadMissingFiles(clientService.Modpacks.Items.First());
            });
            DownloadMissing.ThrownExceptions.Subscribe(e => throw e);

            var ds = clientService.DownloadService.Downloads.Connect();
            ds
                .AutoRefresh(d => d.State)
                .Filter(d => d.State == DownloadState.Downloading)
                .Transform(d => new DownloadViewModel(d))
                .Bind(downloads)
                .Subscribe();
            queueCount = ds
                .AutoRefresh(d => d.State)
                .Filter(d => d.State == DownloadState.Queued)
                .Count()
                .Select(i => "file".ToQuantity(i))
                .ToProperty(this, t => t.QueueCount);
        }

        public ReactiveCommand DownloadMissing { get; }
        public IReadOnlyReactiveList<DownloadViewModel> Downloads => downloads;
        public string QueueCount => queueCount.Value;

        public string Url
        {
            get => url;
            set => this.RaiseAndSetIfChanged(ref url, value);
        }
    }
}