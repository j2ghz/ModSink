using System;
using System.Collections.Generic;
using System.Text;

namespace ModSink.Core.Client
{
    public interface IDownloadManager
    {
        ICollection<IDownload> Downloads { get; }
    }
}