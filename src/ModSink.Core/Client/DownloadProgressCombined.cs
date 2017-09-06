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
        public readonly ulong Speed;

        public DownloadProgressCombined(DownloadProgress current, DownloadProgress previous)
        {
            this.Current = current;
            this.Previous = previous;
            this.Elapsed = this.Current.Timestamp - this.Previous.Timestamp;
            var downloaded = this.Current.Downloaded - previous.Downloaded;
            if (this.Elapsed.Ticks != 0)
            {
                this.Speed = (downloaded * TimeSpan.TicksPerSecond) / (ulong)this.Elapsed.Ticks;
            }
            else
            {
                this.Speed = 0;
            }
        }
    }
}