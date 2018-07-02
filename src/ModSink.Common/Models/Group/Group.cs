using System;
using System.Collections.Generic;

namespace ModSink.Common.Models.Group
{
    [Serializable]
    public class Group : IBaseUri
    {
        public ICollection<RepoInfo> RepoInfos { get; set; }
        public Uri BaseUri { get; set; }
    }
}