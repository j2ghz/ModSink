using System;
using System.Collections.Generic;

namespace ModSink.Domain.Entities.Repo
{
    public class Repo
    {
        public IEnumerable<OnlineFile> Files { get; private set; }
    }
}