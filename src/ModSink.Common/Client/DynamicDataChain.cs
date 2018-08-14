using System;
using System.Linq;
using DynamicData;
using ModSink.Common.Models.Client;
using ModSink.Common.Models.Repo;

namespace ModSink.Common.Client
{
    public static class DynamicDataChain
    {
        public static IObservableCache<FileSignature, FileSignature> GetDownloadsFromModpacks(
            IObservableCache<Modpack, Guid> modpacks)
        {
            return modpacks.Connect()
                .AutoRefresh(m => m.Selected)
                .Filter(m => m.Selected)
                .TransformMany(m => m.Mods, m => m.Mod.Id)
                .TransformMany(m => m.Mod.Files.Values, fs => fs)
                .AsObservableCache();
        }

        public static IObservableCache<Modpack, Guid> GetModpacksFromRepos(IConnectableCache<Repo, Uri> repos)
        {
            return repos.Connect()
                .TransformMany(r => r.Modpacks, m => m.Id)
                .AsObservableCache();
        }

        public static IObservableCache<OnlineFile, FileSignature> GetOnlineFileFromRepos(
            IConnectableCache<Repo, Uri> repos)
        {
            return repos.Connect()
                .TransformMany(
                    repo => repo.Files.Select(kvp => new OnlineFile(kvp.Key, new Uri(repo.BaseUri, kvp.Value))),
                    of => of.FileSignature)
                .AsObservableCache();
        }
    }
}