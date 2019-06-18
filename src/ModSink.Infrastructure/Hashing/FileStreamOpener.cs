using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text;
using Microsoft.Extensions.Options;
using ModSink.Application.Hashing;

namespace ModSink.Infrastructure.Hashing
{
    public class FileStreamOpener : IFileOpener
    {
        private readonly IOptionsMonitor<Options> _optionsMonitor;

        public class Options
        {
            public int BufferSize { get; set; }
            public bool UseAsync { get; set; }
        }

        public FileStreamOpener(IOptionsMonitor<Options> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
        }
        public Stream OpenRead(IFileInfo file)
        {
            return new FileStream(file.FullName,FileMode.Open,FileAccess.Read,FileShare.Read,_optionsMonitor.CurrentValue.BufferSize,_optionsMonitor.CurrentValue.UseAsync);
        }
    }
}
