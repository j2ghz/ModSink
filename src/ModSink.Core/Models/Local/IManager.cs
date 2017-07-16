using System;
using System.Collections.Generic;
using System.Text;

namespace ModSink.Core.Models.Local
{
    public interface IManager
    {
        void LinkFile(Hash hash, string path);
    }
}