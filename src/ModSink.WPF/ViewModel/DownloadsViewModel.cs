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
        private readonly ObservableAsPropertyHelper<string> status;

        public DownloadsViewModel(ClientService clientService)
        {
            clientService.ActiveDownloads.Connect()
                .Transform(d => new DownloadViewModel(d))
                .DisposeMany()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(Downloads)
                .Subscribe()
                .DisposeWith(disposable);
            clientService.QueuedDownloads.Connect()
                .Transform(qd=>qd.FileSignature.Hash.ToString())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(Queue)
                .Subscribe()
                .DisposeWith(disposable);
            status = clientService.QueuedDownloads.CountChanged
                .CombineLatest(clientService.ActiveDownloads.CountChanged, (queue, active) =>
                    $"Downloading {"file".ToQuantity(active)}, {"file".ToQuantity(queue)} in queue")

                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, t => t.Status, scheduler: RxApp.MainThreadScheduler);
        }

        public IObservableCollection<string> Queue { get; } =
            new ObservableCollectionExtended<string>();

        public ObservableCollectionExtended<DownloadViewModel> Downloads { get; } =
            new ObservableCollectionExtended<DownloadViewModel>();

        public string Status => status.Value;

        public void Dispose()
        {
            disposable?.Dispose();
        }
    }
}