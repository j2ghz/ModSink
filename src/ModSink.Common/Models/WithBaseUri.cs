using System;

namespace ModSink.Common.Models
{
    [Serializable]
    public abstract class WithBaseUri
    {
        public Uri BaseUri { get; set; }

        public Uri CombineBaseUri(Uri relative)
        {
            return new Uri(BaseUri, relative);
        }
    }
}