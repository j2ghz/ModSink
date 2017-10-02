using ModSink.Core.Client;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModSink.WPF.ViewModel
{
    public class DownloadsViewModel : ReactiveObject
    {
        public DownloadsViewModel(IDownloadManager downloadManager)
        {
            this.DownloadManager = downloadManager;
        }

        public IDownloadManager DownloadManager { get; }
    }
}