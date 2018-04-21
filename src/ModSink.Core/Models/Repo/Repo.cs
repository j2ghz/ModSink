using System;
using System.Collections.Generic;

namespace ModSink.Core.Models.Repo
{
    [Serializable]
    public class Repo : IBaseUri
    {
        public Uri BaseUri { get; set; }

        /// <summary>
        /// <see cref="string"/> is relative path
        /// </summary>
        public IDictionary<FileSignature, Uri> Files { get; set; }

        public ICollection<Modpack> Modpacks { get; set; }
    }
}