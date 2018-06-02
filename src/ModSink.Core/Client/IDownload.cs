using System;
using System.IO;
using System.Threading.Tasks;
using ReactiveUI;

namespace ModSink.Core.Client
{
    public enum DownloadState
    {
        Queued,
        Downloading,
        Errored,
        Finished
    }

    public interface IDownload : IReactiveObject
    {
        Lazy<Task<Stream>> Destination { get; }
        string Name { get; }
        IObservable<DownloadProgress> Progress { get; }
        Uri Source { get; }
        DownloadState State { get; }

        /// <summary>
        ///     In bytes, as <see cref="FileInfo" />. Cancelled if different, ignored if zero.
        /// </summary>
        ulong ExpectedLength { get; }

        Task Start(IDownloader downloader);
    }
}