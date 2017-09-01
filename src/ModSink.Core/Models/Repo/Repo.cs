using System;
using System.Collections.Generic;

namespace ModSink.Core.Models.Repo
{
    [Serializable]
    public class Repo
    {
        /// <summary>
        /// <see cref="string"/> is relative path
        /// </summary>
        public IDictionary<HashValue, Uri> Files { get; set; }

        public ICollection<Modpack> Modpacks { get; set; }
    }
}