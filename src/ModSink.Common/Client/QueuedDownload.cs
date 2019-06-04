using System;
using System.Collections.Generic;
using System.Text;
using Anotar.Serilog;
using ModSink.Common.Models.DTO.Repo;

namespace ModSink.Common.Client
{
    public readonly struct QueuedDownload
    {
        public readonly FileSignature FileSignature;
        public readonly Uri Source;

        public QueuedDownload(FileSignature fileSignature, Uri source)
        {
            FileSignature = fileSignature;
            Source = source;
        }
    }
}
