using System.Runtime.Serialization.Formatters.Binary;
using ModSink.Common;
using ModSink.WPF.Helpers;
using ModSink.WPF.Model;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ModSink.WPF.ViewModel
{
    public class AppBootstrapper : ReactiveObject
    {
        public AppBootstrapper()
        {
            var modsink = new ModSinkBuilder()
                .WithDownloader(new HttpClientDownloader())
                .WithFormatter(new BinaryFormatter())
                .InDirectory(PathProvider.Downloads)
                .Build();
            var cs = modsink.Client;
            DownloadsVM = new DownloadsViewModel(cs);
            LibraryVM = new LibraryViewModel(cs);
            SettingsVM = new SettingsViewModel(new SettingsModel(cs));
        }

        [Reactive] public SettingsViewModel SettingsVM { get; set; }
        [Reactive] public DownloadsViewModel DownloadsVM { get; set; }
        [Reactive] public LibraryViewModel LibraryVM { get; set; }
    }
}