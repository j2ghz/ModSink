using ModSink.Core.Models;
using ModSink.Core.Models.Local;
using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Text;
using System.Threading;

namespace ModSink.Common
{
    [Export(typeof(IModSink))]
    public class ModSink : IModSink
    {
        [Import]
        public IHashFunction HashFunction { get; }

        [Import]
        public IManager Manager { get; }
    }
}