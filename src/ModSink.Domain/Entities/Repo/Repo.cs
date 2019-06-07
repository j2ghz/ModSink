using System;
using System.Collections.Generic;
using Multiformats.Hash;

namespace ModSink.Domain.Entities.Repo
{
    public class Repo
    {
        public IDictionary<Multihash, Uri> Files { get; }
    }
}