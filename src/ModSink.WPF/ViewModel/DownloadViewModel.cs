using System;
using System.Linq;
using System.Reactive.Linq;
using Anotar.Serilog;
using Humanizer;
using ModSink.Common.Client;
using ReactiveUI;

namespace ModSink.WPF.ViewModel
{
    public class DownloadViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<string> downloaded;
        private readonly ObservableAsPropertyHelper<double> progress;
        private readonly ObservableAsPropertyHelper<string> size;
        private readonly ObservableAsPropertyHelper<string> speed;
        private readonly ObservableAsPropertyHelper<string> state;

        public DownloadViewModel(ActiveDownload activeDownload)
        {
            Name = activeDownload.Name;
            var dp = activeDownload.Progress
                .Sample(TimeSpan.FromMilliseconds(100))
                .Buffer(2, 1)
                .Select(progList => new DownloadProgressCombined(progList.Last(), progList.First()));

            speed = dp.Select(p => p.Speed.Humanize("G03")).ToProperty(this, x => x.Speed);

            downloaded = activeDownload.Progress
                .Sample(TimeSpan.FromMilliseconds(250)).Select(p => p.Downloaded.Humanize("G03"))
                .ToProperty(this, x => x.Downloaded);

            var dpRealtime = activeDownload.Progress
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

        public string State => state.Value;
        public string Downloaded => downloaded.Value;
        public string Name { get; }
        public double Progress => progress.Value;
        public string Size => size.Value;
        public string Speed => speed.Value;

        private void LogErrors(IHandleObservableErrors oaph)
        {
            oaph.ThrownExceptions.Subscribe(e =>
                LogTo.Error(e, "[{download}] failed", this.Name));
        }
    }
}