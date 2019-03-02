using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Humanizer;
using ModSink.Common.Client;
using ReactiveUI;

namespace ModSink.UI.ViewModel
{
    public class DownloadViewModel : ReactiveObject, IDisposable
    {
        private readonly CompositeDisposable disposable = new CompositeDisposable();
        private readonly ObservableAsPropertyHelper<string> downloaded;
        private readonly ObservableAsPropertyHelper<double> progress;
        private readonly ObservableAsPropertyHelper<string> size;
        private readonly ObservableAsPropertyHelper<string> speed;
        private readonly ObservableAsPropertyHelper<DownloadProgress.TransferState> state;
        private readonly ObservableAsPropertyHelper<string> status;

        public DownloadViewModel(ActiveDownload activeDownload)
        {
            Name = activeDownload.Name;
            var dp = activeDownload.Progress
                .Sample(TimeSpan.FromMilliseconds(100))
                .Buffer(2, 1)
                .Select(progList => new DownloadProgressCombined(progList.Last(), progList.First()));

            speed = LogErrorsAndDispose(dp
                .Select(p => p.Speed.Humanize("G03"))
                .ToProperty(this, x => x.Speed, scheduler: RxApp.MainThreadScheduler));

            downloaded = LogErrorsAndDispose(activeDownload.Progress
                .Sample(TimeSpan.FromMilliseconds(250))
                .Select(p => p.Downloaded.Humanize("G03"))
                .ToProperty(this, x => x.Downloaded, scheduler: RxApp.MainThreadScheduler));

            var dpRealtime = activeDownload.Progress
                .Sample(TimeSpan.FromSeconds(1.0 / 60));

            progress = LogErrorsAndDispose(dpRealtime
                .Where(p => p.Size.Bits > 0)
                .Select(p => 100d * p.Downloaded.Bits / p.Size.Bits)
                .ToProperty(this, x => x.Progress, scheduler: RxApp.MainThreadScheduler));
            size = LogErrorsAndDispose(dpRealtime
                .Select(p => p.Size.Humanize("G03"))
                .ToProperty(this, x => x.Size, scheduler: RxApp.MainThreadScheduler));
            state = LogErrorsAndDispose(dpRealtime
                .Select(p => p.State)
                .ToProperty(this, x => x.State, scheduler: RxApp.MainThreadScheduler));
            status = LogErrorsAndDispose(dp
                .Select(p =>
                    $"{p.Current.Downloaded.Humanize("G03")}/{p.Current.Size.Humanize("G03")} @ {p.Speed.Humanize("G03")}")
                .ToProperty(this, x => x.Status, scheduler: RxApp.MainThreadScheduler));
        }

        public DownloadProgress.TransferState State => state.Value;
        public string Downloaded => downloaded.Value;
        public string Name { get; }

        public string Status => status.Value;
        public double Progress => progress.Value;
        public string Size => size.Value;
        public string Speed => speed.Value;

        public void Dispose()
        {
            disposable?.Dispose();
        }

        private ObservableAsPropertyHelper<T> LogErrorsAndDispose<T>(ObservableAsPropertyHelper<T> oaph)
        {
            oaph.DisposeWith(disposable);
            return oaph;
        }
    }
}