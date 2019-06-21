using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text;

namespace ModSink.Application.Hashing
{
    public interface IFileOpener
    {
        Stream OpenRead(IFileInfo file);
    }
}
