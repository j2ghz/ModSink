using System;
using System.Collections.Generic;
using System.Text;

namespace ModSink.CLI.Verbs
{
    public abstract class BaseVerb<TOpts>
    {
        public abstract int Run(TOpts options);
    }
}
