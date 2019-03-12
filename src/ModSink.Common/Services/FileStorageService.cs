using System.IO;
using DynamicData;
using ModSink.Common.Client;
using ModSink.Common.Models.DTO.Repo;

namespace ModSink.Common.Services
{
    public class FileStorageService
    {
        private readonly SourceCache<FileSignature, FileSignature> availableFiles =
            new SourceCache<FileSignature, FileSignature>(x => x);

        private readonly IFileAccessService fileAccessService;


        public FileStorageService(IFileAccessService fileAccessService)
        {
            this.fileAccessService = fileAccessService;
            availableFiles.AddOrUpdate(fileAccessService.FilesAvailable());
        }

        public IConnectableCache<FileSignature, FileSignature> AvailableFiles => availableFiles;

        public void FinishDownload(FileSignature fileSignature)
        {
            fileAccessService.TemporaryFinished(fileSignature);
            availableFiles.AddOrUpdate(fileSignature);
        }

        public Stream StartDownload(FileSignature fileSignature)
        {
            return fileAccessService.Write(fileSignature, true);
        }
    }
}