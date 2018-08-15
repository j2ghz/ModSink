using System;
using System.Collections.Generic;

namespace ModSink.Common.Models.Group
{
    [Serializable]
    public class Group : WithBaseUri
    {
        public ICollection<RepoInfo> RepoInfos { get; set; }
    }
}