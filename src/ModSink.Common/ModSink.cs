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
        private readonly IHashFunction hashFunction;

        [ImportingConstructor]
        public ModSink(IHashFunction hashFunction)
        {
            this.hashFunction = hashFunction;
        }
    }
}