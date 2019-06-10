using System;
using System.Collections.Generic;
using System.Text;
using Multiformats.Hash;

namespace ModSink.Domain.Entities.Repo
{
    public class OnlineFile
    {
        public Multihash Hash { get; private set; }
        public RelativeUri RelativeUri { get; private set; }
    }
}
