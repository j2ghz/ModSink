using System;
using DynamicData;

namespace ModSink.Core.Client
{
    public interface IDownloadService
    {
        IObservableCache<IDownload, Guid> Downloads { get; }

        void Add(IDownload download);
    }
}