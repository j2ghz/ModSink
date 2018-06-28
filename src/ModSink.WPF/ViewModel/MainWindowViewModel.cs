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
            var cs = new ClientService(new DownloadService(new HttpClientDownloader()),
                new LocalStorageService(PathProvider.Downloads),
                new HttpClientDownloader(), new BinaryFormatter());
            DownloadsVM = new DownloadsViewModel(cs);
            LibraryVM = new LibraryViewModel(cs);
            SettingsVM = new SettingsViewModel(new SettingsModel(cs));
        }

        public SettingsViewModel SettingsVM { get; set; }
        public DownloadsViewModel DownloadsVM { get; }
        public LibraryViewModel LibraryVM { get; }
    }
}