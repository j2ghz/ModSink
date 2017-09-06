using System;

namespace ModSink.Core.Client
{
    public struct DownloadProgress
    {
        public readonly ulong Downloaded;
        public readonly ulong Size;
        public readonly DownloadState State;
        public readonly DateTime Timestamp;

        public DownloadProgress(ulong size, ulong downloaded, DownloadState state)
        {
            this.Size = size;
            this.Downloaded = downloaded;
            this.State = state;
            this.Timestamp = DateTime.Now;
        }

        public enum DownloadState
        {
            AwaitingResponse,
            ReadingResponse,
            Downloading,
            Finished
        }

        public ulong Remaining => this.Size - this.Downloaded;
    }
}