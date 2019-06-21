using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text;
using ModSink.Domain.Entities.File;

namespace ModSink.Application.Hashing
{
    public interface IHashingService
    {
        IObservable<Hash> GetFileHashes(IDirectoryInfo directory);
    }
}
