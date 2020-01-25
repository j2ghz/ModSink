using System.IO;
using System.IO.Abstractions;
using Microsoft.Extensions.Options;
using ModSink.Application.Hashing;

namespace ModSink.Infrastructure.Hashing
{
    public class FileStreamOpener : IFileOpener
    {
        private readonly IFileStreamFactory _fileStreamFactory;

        private readonly IOptions<Options> _options;

        public FileStreamOpener(IOptions<Options> options, IFileStreamFactory fileStreamFactory)
        {
            _options = options;
            _fileStreamFactory = fileStreamFactory;
        }

        public Stream OpenRead(IFileInfo file)
        {
            return _fileStreamFactory.Create(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read,
                _options.Value.BufferSize, _options.Value.UseAsync);
        }

        public class Options
        {
            public int BufferSize { get; set; } = 4096;
            public bool UseAsync { get; set; } = false;
        }
    }
}
