using System;
using Humanizer;
using Humanizer.Bytes;

namespace ModSink.Common.Client
{
    public struct DownloadProgressCombined
    {
        public readonly DownloadProgress Current;
        public readonly TimeSpan Elapsed;
        public readonly DownloadProgress Previous;
        public readonly ByteRate Speed;

        public DownloadProgressCombined(DownloadProgress current, DownloadProgress previous)
        {
            Current = current;
            Previous = previous;
            Elapsed = Current.Timestamp - Previous.Timestamp;
            var downloaded = Current.Downloaded.Subtract(previous.Downloaded);
            Speed = downloaded.Per(Elapsed);
        }
    }
}