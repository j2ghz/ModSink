using System;
using System.Collections.Generic;
using System.Text;

namespace ModSink.Core.Client
{
    public struct DownloadProgress
    {
        public readonly ulong Downloaded;
        public readonly ulong Size;
        public readonly DownloadState State;

        public DownloadProgress(ulong size, ulong downloaded, DownloadState state)
        {
            Size = size;
            Downloaded = downloaded;
            State = state;
        }

        public enum DownloadState
        {
            AwaitingResponse,
            ReadingResponse,
            Downloading,
            Finished
        }

        public ulong Remaining => Size - Downloaded;
    }
}