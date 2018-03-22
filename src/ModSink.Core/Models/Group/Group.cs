using System;
using System.Collections.Generic;

namespace ModSink.Core.Models.Group
{
    public class Group : IBaseUri
    {
        public IEnumerable<RepoInfo> RepoInfos { get; set; }
        public Uri BaseUri { get; set; }
    }
}