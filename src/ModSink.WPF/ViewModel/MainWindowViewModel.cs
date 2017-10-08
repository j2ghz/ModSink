using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModSink.WPF.ViewModel
{
    public class MainWindowViewModel
    {
        public MainWindowViewModel(DownloadsViewModel downloadsVM, LibraryViewModel libraryVM)
        {
            DownloadsVM = downloadsVM;
            LibraryVM = libraryVM;
        }

        public DownloadsViewModel DownloadsVM { get; }
        public LibraryViewModel LibraryVM { get; }
    }
}