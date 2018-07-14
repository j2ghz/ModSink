using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using Humanizer;
using ModSink.Common.Client;
using ReactiveUI;

namespace ModSink.WPF.ViewModel
{
    public class DownloadsViewModel : ReactiveObject, IDisposable
    {
        private readonly CompositeDisposable disposable = new CompositeDisposable();
        private readonly ObservableAsPropertyHelper<string> queueCount;

        public DownloadsViewModel(ClientService clientService)
        {
            clientService.ActiveDownloads.Connect()
                .Transform(d => new DownloadViewModel(d))
                .DisposeMany()
                .Bind(Downloads)
                .Subscribe()
                .DisposeWith(disposable);
            queueCount = clientService.QueuedDownloads.CountChanged
                .Select(i => "file".ToQuantity(i))
                .ToProperty(this, t => t.QueueCount);
        }


        public ObservableCollectionExtended<DownloadViewModel> Downloads { get; } =
            new ObservableCollectionExtended<DownloadViewModel>();

        public string QueueCount => queueCount.Value;

        public void Dispose()
        {
            disposable?.Dispose();
        }
    }
}