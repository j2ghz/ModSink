using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using DynamicData;
using ModSink.Core.Client;
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

            Repos = RepoUrls.Connect().Transform(s => new Uri(s)).TransformAsync(LoadRepo).AsObservableList();
            Modpacks = Repos.Connect().TransformMany(r => r.Modpacks).AsObservableList();
        }

        public IDownloader Downloader { get; }
        public IFormatter SerializationFormatter { get; }
        private ILogger Log => Serilog.Log.ForContext<ClientService>();
        public ISourceList<string> RepoUrls { get; } = new SourceList<string>();
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

        public async Task<Repo> LoadRepo(Uri uri)
        {
            Log.Information("Loading repo from {url}", uri);
            var tempFile = Path.GetTempFileName();
            Log.Debug("Downloading repo to temp file {path}", tempFile);
            var stream = new FileStream(tempFile, FileMode.Create);
            await Downloader.Download(uri, stream);
            stream = new FileStream(tempFile, FileMode.Open, FileAccess.Read);
            Log.Debug("Deserializing downloaded repo");
            var repo = (Repo) SerializationFormatter.Deserialize(stream);
            repo.BaseUri = new Uri(uri, ".");
            return repo;
        }
    }
}