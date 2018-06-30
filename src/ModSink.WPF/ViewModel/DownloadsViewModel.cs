using System;
using System.Reactive.Linq;
using Anotar.Serilog;
using DynamicData;
using DynamicData.Aggregation;
using DynamicData.Binding;
using Humanizer;
using ModSink.Core.Client;
using ReactiveUI;

namespace ModSink.WPF.ViewModel
{
    public class DownloadsViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<string> queueCount;

        public DownloadsViewModel(IClientService clientService)
        {
            var ds = clientService.DownloadService.Downloads.Connect();
            ds
                .AutoRefresh(d => d.State)
                .Filter(d => d.State == DownloadState.Downloading)
                .Transform(d => new DownloadViewModel(d))
                .DisposeMany()
                .Bind(Downloads)
                .Subscribe();
            queueCount = ds
                .AutoRefresh(d => d.State)
                .Filter(d => d.State == DownloadState.Queued)
                .Count()
                .Select(i => "file".ToQuantity(i))
                .ToProperty(this, t => t.QueueCount);
            queueCount.ThrownExceptions.Subscribe(e => LogTo.Warning(e, "Download failed"));
        }


        public ObservableCollectionExtended<DownloadViewModel> Downloads { get; } =
            new ObservableCollectionExtended<DownloadViewModel>();

        public string QueueCount => queueCount.Value;
    }
}