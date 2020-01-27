using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ModSink.Application.Download {
  public interface IDownloader {
    bool CanDownload(Uri uri);
    Task<Stream>DownloadAsync(Uri uri, CancellationToken cancellationToken);
  }
}
