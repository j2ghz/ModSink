using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ModSink.Domain.Entities;

namespace ModSink.Application
{
    public interface IDownloader
    {
        bool CanDownload(Uri uri);
        Task<Stream> DownloadAsync(Uri uri, CancellationToken cancellationToken);
    }
}
