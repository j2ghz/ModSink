using System;
using System.Collections.Generic;
using System.Text;

namespace ModSink.Domain.Entities.Repo
{
    public class OnlineFile
    {
        public string Hash { get; private set; }
        public RelativeUri RelativeUri { get; private set; }
    }
}
