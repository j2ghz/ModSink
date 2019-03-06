using System;
using System.Linq;
using System.Threading.Tasks;
using Anotar.Serilog;
using DynamicData;
using ModSink.Common.Models.Client;
using ModSink.Common.Models.Group;
using ModSink.Common.Models.Repo;

namespace ModSink.Common.Client
{
    public class DynamicDataChain
    {
        public static IObservable<IChangeSet<FileSignature, FileSignature>> GetDownloadsFromModpacks(
            IObservable<IChangeSet<Modpack, Guid>> modpacks)
        {
            return modpacks
                .AutoRefresh(m => m.Selected)
                .Filter(m => m.Selected)
                .TransformMany(m => m.Mods, m => m.Mod.Id)
                .TransformMany(m => m.Mod.Files.Values, fs => fs);
        }

        public static IObservable<IChangeSet<Modpack, Guid>> GetModpacksFromRepos(
            IObservable<IChangeSet<Repo, Uri>> repos)
        {
            return repos
                .TransformMany(r => r.Modpacks, m => m.Id);
        }
    }
}