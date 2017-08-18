using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ModSink.Core.Models.Repo;
using System.IO;

namespace ModSink.Core.Models
{
    public interface ISerializer
    {
        Repo.Repo Deserialize(string repo);

        Task<Repo.Repo> Deserialize(Stream stream);

        string Serialize(Repo.Repo repo);

        Task Serialize(Repo.Repo repo, Stream stream);
    }
}