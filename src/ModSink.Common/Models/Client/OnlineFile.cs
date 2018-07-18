using System;
using ModSink.Common.Models.Repo;

namespace ModSink.Common.Models.Client
{
    public readonly struct OnlineFile
    {
        public OnlineFile(FileSignature fileSignature, Uri uri)
        {
            FileSignature = fileSignature;
            Uri = uri;
            if (!Uri.IsAbsoluteUri) throw new UriNotAbsoluteException();
        }

        public FileSignature FileSignature { get; }
        public Uri Uri { get; }
    }
}