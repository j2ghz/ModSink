using System;
using System.Linq;
using System.Reactive.Linq;
using Humanizer;
using ModSink.Core.Client;
using ReactiveUI;
using Serilog;

namespace ModSink.WPF.ViewModel
{
    public class DownloadViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<string> downloaded;
        private readonly ObservableAsPropertyHelper<double> progress;
        private readonly ObservableAsPropertyHelper<string> size;
        private readonly ObservableAsPropertyHelper<string> speed;
        private readonly ObservableAsPropertyHelper<string> state;

        public DownloadViewModel(IDownload download)
        {
            Name = download.Name;
            var dp = download.Progress
                .Sample(TimeSpan.FromMilliseconds(100))
                .Buffer(2, 1)
                .Select(progList => new DownloadProgressCombined(progList.Last(), progList.First()));

            speed = dp.Select(p => p.Speed.Humanize("G03")).ToProperty(this, x => x.Speed);

            downloaded = download.Progress
                .Sample(TimeSpan.FromMilliseconds(250)).Select(p => p.Downloaded.Humanize("G03"))
                .ToProperty(this, x => x.Downloaded);

            var dpRealtime = download.Progress
                .Sample(TimeSpan.FromSeconds(1.0 / 60));


            progress = dpRealtime.Where(p => p.Size.Bits > 0).Select(p => 100d * p.Downloaded.Bits / p.Size.Bits)
                .ToProperty(this, x => x.Progress);
            size = dpRealtime.Select(p => p.Size.Humanize("G03")).ToProperty(this, x => x.Size);
            state = dpRealtime.Select(p => p.State.Humanize()).ToProperty(this, x => x.State);

            LogErrors(downloaded);
            LogErrors(progress);
            LogErrors(size);
            LogErrors(speed);
            LogErrors(state);
        }

        private ILogger log => Log.ForContext<DownloadViewModel>().ForContext("downloadName", Name);
        public string State => state.Value;
        public string Downloaded => downloaded.Value;
        public string Name { get; }
        public double Progress => progress.Value;
        public string Size => size.Value;
        public string Speed => speed.Value;

        private void LogErrors(IHandleObservableErrors oaph)
        {
            oaph.ThrownExceptions.Subscribe(e =>
                log.Error(e, "{downloadName} An exception from Observable of underlying download was caught"));
        }
    }
}