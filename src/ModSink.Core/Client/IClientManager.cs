using ModSink.Core.Models.Repo;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;

namespace ModSink.Core.Local
{
    public interface IClientManager
    {
        Task DownloadMissingFiles(Modpack modpack);

        IObservable<HashValue> GetMissingFiles(Modpack modpack);

        IObservable<Modpack> GetModpacks();
    }
}