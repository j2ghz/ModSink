using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using DynamicData;
using ModSink.Core.Client;
using ModSink.Core.Models;
using ModSink.Core.Models.Group;
using ModSink.Core.Models.Repo;
using ReactiveUI;
using Serilog;

namespace ModSink.Common.Client
{
    public class ClientService : ReactiveObject, IClientService
    {
        public ClientService(IDownloadService downloadService, ILocalStorageService localStorageService,
            IDownloader downloader, IFormatter serializationFormatter)
        {
            DownloadService = downloadService;
            LocalStorageService = localStorageService;
            Downloader = downloader;
            SerializationFormatter = serializationFormatter;

            Groups = GroupUrls
                .Connect()
                .Transform(g => new Uri(g))
                .TransformAsync(Load<Group>)
                .AsObservableList();
            Repos = Groups
                .Connect()
                .TransformMany(g => g.RepoInfos.Select(r => new Uri(g.BaseUri, r.Uri)))
                .TransformAsync(Load<Repo>)
                .AsObservableList();
            Modpacks = Repos
                .Connect()
                .TransformMany(r => r.Modpacks)
                .AsObservableList();
        }

        public IDownloader Downloader { get; }
        public IFormatter SerializationFormatter { get; }
        private ILogger log => Log.ForContext<ClientService>();
        public IObservableList<Group> Groups { get; }
        public ISourceList<string> GroupUrls { get; } = new SourceList<string>();
        public IDownloadService DownloadService { get; }
        public ILocalStorageService LocalStorageService { get; }
        public IObservableList<Modpack> Modpacks { get; }
        public IObservableList<Repo> Repos { get; }


        public async Task DownloadMissingFiles(Modpack modpack)
        {
            Log.Information("Gathering files to download for {modpack}", modpack.Name);
            foreach (var mod in modpack.Mods)
            foreach (var fh in mod.Mod.Files)
            {
                var fileSignature = fh.Value;
                var (available, stream) = await LocalStorageService.WriteIfMissingOrInvalid(fileSignature);
                Log.Debug("Check {fh}, Exists: {exists}", fileSignature.Hash, available);
                if (!available)
                    DownloadService.Add(new Download(GetDownloadUri(fileSignature), stream,
                        fileSignature.ToString()));
            }
        }

        public Uri GetDownloadUri(FileSignature fileSignature)
        {
            foreach (var repo in Repos.Items)
                if (repo.Files.TryGetValue(fileSignature, out var relativeUri))
                    return new Uri(repo.BaseUri, relativeUri);
            throw new KeyNotFoundException($"Key {fileSignature} was not found in a Files dictionary of any Repo");
        }

        private async Task<T> Load<T>(Uri uri) where T : IBaseUri
        {
            var tempFile = Path.GetTempFileName();
            log.Information("Loading {T} from {url} to {path}", typeof(T), uri, tempFile);
            var stream = new FileStream(tempFile, FileMode.Create);
            await Downloader.Download(uri, stream);
            stream = new FileStream(tempFile, FileMode.Open, FileAccess.Read);
            log.Debug("Deserializing");
            var t = (T) SerializationFormatter.Deserialize(stream);
            t.BaseUri = new Uri(uri, ".");
            return t;
        }
    }
}