using ModSink.Common.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ModSink.Core.Client;
using ReactiveUI;
using System.Reactive.Linq;
using Humanizer;

namespace ModSink.WPF.ViewModel
{
    public class DownloadViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<string> downloaded;
        private readonly ObservableAsPropertyHelper<double> progress;
        private readonly ObservableAsPropertyHelper<string> size;
        private readonly ObservableAsPropertyHelper<string> speed;

        public DownloadViewModel(IDownload download)
        {
            Name = download.Name;
            var progress = download.Progress
                .Sample(TimeSpan.FromMilliseconds(250))
                .Buffer(2, 1)
                .Select(progList => new DownloadProgressCombined(progList.Last(), progList.First()));

            this.downloaded = progress.Select(p => p.Current.Downloaded.Humanize("#.##")).ToProperty(this, x => x.Downloaded);
            this.progress = progress.Select(p => 100d * p.Current.Downloaded.Bits / p.Current.Size.Bits).ToProperty(this, x => x.Progress);
            this.size = progress.Select(p => p.Current.Size.Humanize("#.##")).ToProperty(this, x => x.Size);
            this.speed = progress.Select(p => p.Speed.Humanize("#.##")).ToProperty(this, x => x.Speed);
        }

        public string Downloaded => downloaded.Value;
        public string Name { get; }
        public double Progress => progress.Value;
        public string Size => size.Value;
        public string Speed => speed.Value;
    }
}