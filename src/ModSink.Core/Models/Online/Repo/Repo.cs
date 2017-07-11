using System;
using System.Collections.Generic;
using System.Text;

namespace ModSink.Core.Models.Online.Repo
{
    public class Repo
    {
        public IEnumerable<Modpack> Modpacks { get; set; }

        /// <summary>
        /// <see cref="byte"/>[] is hash, <see cref="string"/> is relative path
        /// </summary>
        public IDictionary<byte[],string> Files { get; set; }
    }
}
