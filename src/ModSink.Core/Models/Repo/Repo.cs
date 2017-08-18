using System.Collections.Generic;

namespace ModSink.Core.Models.Repo
{
    public class Repo
    {
        /// <summary>
        /// <see cref="string"/> is relative path
        /// </summary>
        public IDictionary<HashValue, string> Files { get; set; }

        public IList<Modpack> Modpacks { get; set; }
    }
}