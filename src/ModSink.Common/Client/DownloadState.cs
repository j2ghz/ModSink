namespace ModSink.Common.Client
{
    public partial class Download
    {
        public enum DownloadState
        {
            Queued,
            Downloading,
            Errored,
            Finished
        }
    }
}