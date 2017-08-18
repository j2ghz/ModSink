using System;

namespace ModSink.Core.Models.Repo
{
    [Flags]
    public enum ModFlags
    {
        None = 0,
        Optional = 1,
        DefaultOff = 2
    }
}