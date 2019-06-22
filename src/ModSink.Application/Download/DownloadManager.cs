using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace ModSink.Application.Download
{
    public class DownloadManager
    {
        private readonly IEnumerable<IDownloader> _downloaders;
        private readonly List<Task> _downloading = new List<Task>();
        private readonly IOptions<Options> _options;
        private readonly Queue<Uri> _queue = new Queue<Uri>();
        private CancellationTokenSource _cancellationTokenSource;
        private readonly Subject<Unit> _downloadFinished = new Subject<Unit>();

        public DownloadManager(IEnumerable<IDownloader> downloaders, IOptions<Options> options = null)
        {
            _downloaders = downloaders;
            _options = options ?? Microsoft.Extensions.Options.Options.Create(new Options());
        }

        public IObservable<Unit> DownloadFinished => _downloadFinished.AsObservable();

        public int QueueSize => _queue.Count;

        public Task Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            return Task.Run(async () =>
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    if (_downloading.Count < _options.Value.Paralellism)
                    {
                        var uri = _queue.Dequeue();
                        var downloader = _downloaders.Single(d => d.CanDownload(uri))
                            .DownloadAsync(uri, _cancellationTokenSource.Token);
                        _downloading.Add(downloader);
                    }

                    if (_downloading.Any())
                    {
                        await Task.WhenAny(_downloading);
                    }
                    else
                    {
                        _downloadFinished.OnNext(Unit.Default);
                        return;
                    }
                }
            }, _cancellationTokenSource.Token);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }

        public void Add(Uri uri)
        {
            var downloaders = _downloaders.Where(d => d.CanDownload(uri));
            switch (downloaders.Count())
            {
                case 0:
                    throw new Exception($"No downloader available for {uri}");
                    break;
                case 1:
                    _queue.Enqueue(uri);
                    break;
                default:
                    throw new NotImplementedException(
                        "Handling multiple downloaders for one URI scheme not implemented");
            }
        }

        public class Options
        {
            public byte Paralellism { get; set; } = 2;
        }
    }
}