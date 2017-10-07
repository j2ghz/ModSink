using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ModSink.Core.Client
{
    public interface IDownloadService
    {
        IObservableList<IDownload> Downloads { get; }

        void Add(IDownload download);

        void CheckDownloadsToStart();
    }
}