using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using DynamicData;
using ModSink.Common.Models.Repo;
using ReactiveUI;

namespace ModSink.Common.Client
{
    public class LocalFilesManager : ReactiveObject, IDisposable
    {
        private readonly DirectoryInfo localDir;
        private readonly SourceCache<FileSignature,HashValue> filesAvailable;

        public LocalFilesManager(FileAccessService fileAccessService, IObservableList<FileSignature> filesRequired,
            IDownloader downloader)
        {
            filesAvailable.Edit(l =>
            {
                l.AddOrUpdate(localDir.EnumerateFiles()
                    .Select(fi => new FileSignature(new HashValue(fi.Name), fi.Length)));
            });
            //var downloadQueue = filesRequired.Connect()
            //    .Transform(fs => allFiles.Items.First(kvp => kvp.Key.Equals(fs)))
            //    //have a list of files we have, 
            //    .Transform(kvp => new QueuedDownload(kvp.Key, kvp.Value))
            //    .AsObservableList()
            //    .DisposeWith(disposable);
            //downloadQueue.Connect().Subscribe().DisposeWith(disposable);
            //DownloadService =
            //    new DownloadService(downloader, downloadQueue.Connect(), localFilesManager).DisposeWith(disposable);
        }

        public void AddNewFile(FileSignature file)
        {
            filesAvailable.AddOrUpdate(file);

        }

        public FileStream GetTemporaryFileStream(FileSignature fileSignature)
        {
            throw new NotImplementedException();
        }
    }
}
