using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ModSink.Core.Client
{
    public interface IDownloadService
    {
        //TODO: Split into Queue and Active? Or use DynamicData on one source list? OR split interface but use DD on impl?
        IObservableList<IDownload> Queue { get; }

        void Add(IDownload download);

        void CheckDownloadsToStart();
    }
}