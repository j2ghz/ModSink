using System;
using System.Collections.Generic;

namespace ModSink.Common.Models.DTO.Group
{
    [Serializable]
    public class Group : WithBaseUri
    {
        public ICollection<RepoInfo> RepoInfos { get; set; }
    }
}