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

        public static IObservable<IChangeSet<OnlineFile, FileSignature>> GetOnlineFileFromRepos(
            IObservable<IChangeSet<Repo, Uri>> repos)
        {
            return repos
                .TransformMany(
                    repo => repo.Files.Select(kvp => new OnlineFile(kvp.Key, repo.CombineBaseUri(kvp.Value))),
                    of => of.FileSignature);
        }

        public static IObservable<IChangeSet<Repo, Uri>> GetReposFromGroups(
            IObservable<IChangeSet<string, string>> groups,
            Func<Uri, Task<Group>> loadGroup, Func<Uri, Task<Repo>> loadRepo)
        {
            return groups
                .Transform(g => new Uri(g))
                .TransformAsync(loadGroup)
                .TransformMany(g => g.RepoInfos.Select(r => g.CombineBaseUri(r.Uri)), repoUri => repoUri)
                .TransformAsync(loadRepo)
                .OnItemUpdated((repo, _) => LogTo.Information("Repo from {url} has been loaded", repo.BaseUri));
        }
    }
}