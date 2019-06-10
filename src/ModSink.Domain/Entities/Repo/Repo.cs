using System;
using System.Collections.Generic;
using Multiformats.Hash;

namespace ModSink.Domain.Entities.Repo
{
    public class Repo
    {
        public IEnumerable<OnlineFile> Files { get; private set; }
    }
}