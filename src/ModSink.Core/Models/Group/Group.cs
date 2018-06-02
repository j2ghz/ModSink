using System;
using System.Collections.Generic;

namespace ModSink.Core.Models.Group
{
    [Serializable]
    public class Group : IBaseUri
    {
        public ICollection<RepoInfo> RepoInfos { get; set; }
        public Uri BaseUri { get; set; }
    }
}