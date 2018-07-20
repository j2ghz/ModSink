using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Anotar.Serilog;
using Humanizer;
using ModSink.Common.Client;
using ReactiveUI;

namespace ModSink.WPF.ViewModel
{
    public class DownloadViewModel : ReactiveObject, IDisposable
    {
        private readonly ObservableAsPropertyHelper<string> downloaded;
        private readonly ObservableAsPropertyHelper<double> progress;
        private readonly ObservableAsPropertyHelper<string> size;
        private readonly ObservableAsPropertyHelper<string> speed;
        private readonly ObservableAsPropertyHelper<DownloadProgress.TransferState> state;
        private readonly ObservableAsPropertyHelper<string> status;
        private readonly CompositeDisposable disposable = new CompositeDisposable();

        public DownloadViewModel(ActiveDownload activeDownload)
        {
            Name = activeDownload.Name;
            var dp = activeDownload.Progress
                .Sample(TimeSpan.FromMilliseconds(100))
                .Buffer(2, 1)
                .Select(progList => new DownloadProgressCombined(progList.Last(), progList.First()));

            speed = dp
                .Select(p => p.Speed.Humanize("G03"))
                .ToProperty(this, x => x.Speed)
                .DisposeWith(disposable);

            downloaded = activeDownload.Progress
                .Sample(TimeSpan.FromMilliseconds(250))
                .Select(p => p.Downloaded.Humanize("G03"))
                .ToProperty(this, x => x.Downloaded)
                .DisposeWith(disposable);

            var dpRealtime = activeDownload.Progress
                .Sample(TimeSpan.FromSeconds(1.0 / 60));
            
            progress = dpRealtime
                .Where(p => p.Size.Bits > 0)
                .Select(p => 100d * p.Downloaded.Bits / p.Size.Bits)
                .ToProperty(this, x => x.Progress)
                .DisposeWith(disposable);
            size = dpRealtime
                .Select(p => p.Size.Humanize("G03"))
                .ToProperty(this, x => x.Size)
                .DisposeWith(disposable);
            state = dpRealtime
                .Select(p => p.State)
                .ToProperty(this, x => x.State)
                .DisposeWith(disposable);
            status = dp
                .Select(p =>
                    $"{p.Current.Downloaded.Humanize("G03")}/{p.Current.Size.Humanize("G03")} @ {p.Speed.Humanize("G03")}")
                .ToProperty(this, x => x.Status)
                .DisposeWith(disposable);

            LogErrors(downloaded);
            LogErrors(progress);
            LogErrors(size);
            LogErrors(speed);
            LogErrors(state);
            LogErrors(status);
        }

        public DownloadProgress.TransferState State => state.Value;
        public string Downloaded => downloaded.Value;
        public string Name { get; }

        public string Status => status.Value;
        public double Progress => progress.Value;
        public string Size => size.Value;
        public string Speed => speed.Value;

        private void LogErrors(IHandleObservableErrors oaph)
        {
            oaph.ThrownExceptions.Subscribe(e =>
                LogTo.Error(e, "[{download}] failed", Name));
        }

        public void Dispose()
        {
            disposable?.Dispose();
        }
    }
}