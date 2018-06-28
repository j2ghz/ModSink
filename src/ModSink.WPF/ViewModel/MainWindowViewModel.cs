using System.Runtime.Serialization.Formatters.Binary;
using ModSink.Common;
using ModSink.Common.Client;
using ModSink.WPF.Helpers;
using ModSink.WPF.Model;
using ReactiveUI;

namespace ModSink.WPF.ViewModel
{
    public class MainWindowViewModel : ReactiveObject
    {
        public MainWindowViewModel()
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

        public SettingsViewModel SettingsVM { get; set; }
        public DownloadsViewModel DownloadsVM { get; }
        public LibraryViewModel LibraryVM { get; }
    }
}