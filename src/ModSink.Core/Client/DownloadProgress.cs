using Humanizer.Bytes;
using System;

namespace ModSink.Core.Client
{
    public struct DownloadProgress
    {
        public readonly ByteSize Downloaded;
        public readonly ByteSize Size;
        public readonly TransferState State;
        public readonly DateTime Timestamp;

        public DownloadProgress(ByteSize size, ByteSize downloaded, TransferState state)
        {
            this.Size = size;
            this.Downloaded = downloaded;
            this.State = state;
            this.Timestamp = DateTime.Now;
        }

        public enum TransferState
        {
            AwaitingResponse,
            ReadingResponse,
            Downloading,
            Finished
        }

        public ByteSize Remaining => this.Size.Subtract(this.Downloaded);
    }
}