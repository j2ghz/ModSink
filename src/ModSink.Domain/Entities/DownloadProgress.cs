using System;
using Humanizer;
using Humanizer.Bytes;

namespace ModSink.Domain.Entities
{
public readonly struct DownloadProgress
{
    public readonly ByteSize Downloaded;
    public readonly ByteSize Size;
    public readonly TransferState State;
    public readonly DateTimeOffset Timestamp;


    /// <param name="state">Current state of the download</param>
    /// <param name="size">Size of the download, zero if not specified</param>
    /// <param name="downloaded">Size of the downloaded part, zero if not specified</param>
    public DownloadProgress(TransferState state, ByteSize size = default, ByteSize downloaded = default)
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

    public override string ToString()
    {
        return
            $"{State.Humanize(LetterCasing.Sentence)} {Downloaded.Humanize("G03")} out of {Size.Humanize("G03")}";
    }

    public ByteSize Remaining => Size.Subtract(Downloaded);
}
}
