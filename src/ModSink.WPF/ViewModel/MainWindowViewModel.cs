namespace ModSink.WPF.ViewModel
{
    public class MainWindowViewModel
    {
        public MainWindowViewModel(DownloadsViewModel downloadsVM, LibraryViewModel libraryVM,
            SettingsViewModel settingsVM)
        {
            DownloadsVM = downloadsVM;
            LibraryVM = libraryVM;
            SettingsVM = settingsVM;
        }

        public SettingsViewModel SettingsVM { get; set; }
        public DownloadsViewModel DownloadsVM { get; }
        public LibraryViewModel LibraryVM { get; }
    }
}