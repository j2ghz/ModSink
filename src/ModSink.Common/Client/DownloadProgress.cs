using System;
using Humanizer.Bytes;

namespace ModSink.Common.Client
{
    public struct DownloadProgress
    {
        public readonly ByteSize Downloaded;
        public readonly ByteSize Size;
        public readonly TransferState State;
        public readonly DateTime Timestamp;

        public DownloadProgress(ByteSize size, ByteSize downloaded, TransferState state)
        {
            Size = size;
            Downloaded = downloaded;
            State = state;
            Timestamp = DateTime.Now;
        }

        public enum TransferState
        {
            NotStarted,
            AwaitingResponse,
            ReadingResponse,
            Downloading,
            Finished
        }

        public ByteSize Remaining => Size.Subtract(Downloaded);
    }
}