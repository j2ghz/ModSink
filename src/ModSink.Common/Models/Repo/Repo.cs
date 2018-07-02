using System;
using System.Collections.Generic;

namespace ModSink.Common.Models.Repo
{
    [Serializable]
    public class Repo : IBaseUri
    {
        /// <summary>
        ///     <see cref="string" /> is relative path
        /// </summary>
        public IDictionary<FileSignature, Uri> Files { get; set; }

        public ICollection<Modpack> Modpacks { get; set; }
        public Uri BaseUri { get; set; }
    }
}