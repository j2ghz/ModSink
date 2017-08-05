using ModSink.Core.Models.Local;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModSink.Core.Models
{
    public interface IModSink
    {
        IHashFunction HashFunction { get; }
    }
}