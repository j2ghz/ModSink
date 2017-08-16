using System.Collections.Generic;

namespace ModSink.Core.Models.Repo
{
    public class Repo
    {
        /// <summary>
        /// <see cref="byte"/>[] is hash, <see cref="string"/> is relative path
        /// </summary>
        public IDictionary<byte[], string> Files { get; set; }

        public IEnumerable<Modpack> Modpacks { get; set; }
    }
}