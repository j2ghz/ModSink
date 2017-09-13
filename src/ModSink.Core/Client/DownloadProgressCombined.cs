using Humanizer;
using Humanizer.Bytes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModSink.Core.Client
{
    public struct DownloadProgressCombined
    {
        public readonly DownloadProgress Current;
        public readonly TimeSpan Elapsed;
        public readonly DownloadProgress Previous;
        public readonly ByteRate Speed;

        public DownloadProgressCombined(DownloadProgress current, DownloadProgress previous)
        {
            this.Current = current;
            this.Previous = previous;
            this.Elapsed = this.Current.Timestamp - this.Previous.Timestamp;
            var downloaded = this.Current.Downloaded.Subtract(previous.Downloaded);
            this.Speed = downloaded.Per(this.Elapsed);
        }
    }
}