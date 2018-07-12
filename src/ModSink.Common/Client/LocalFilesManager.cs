using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using DynamicData;
using ModSink.Common.Models.Client;
using ModSink.Common.Models.Repo;
using ReactiveUI;

namespace ModSink.Common.Client
{
    public class LocalFilesManager : ReactiveObject, IDisposable
    {
        private readonly SourceCache<FileSignature,HashValue> filesAvailable = new SourceCache<FileSignature, HashValue>(fs=>fs.Hash);
        private readonly CompositeDisposable disposable;

        public LocalFilesManager(FileAccessService fileAccessService, IObservableList<FileSignature> filesRequired, IObservableList<OnlineFile> files,
            IDownloader downloader)
        {
            filesAvailable.Edit(l =>
            {
                l.AddOrUpdate(fileAccessService.GetFiles()
                    .Select(fi => new FileSignature(new HashValue(fi.Name), fi.Length)));
            });

            var downloadQueue = filesRequired.Connect()
                .Filter(fs=>filesAvailable.Items.Contains(fs))
                .Transform(fs=>files.Items.Single(onlineFile => onlineFile.FileSignature.Equals(fs)))
                .Transform(of => new QueuedDownload(of.FileSignature,of.Uri))
                .AsObservableList()
                .DisposeWith(disposable);
            downloadQueue.Connect().Subscribe().DisposeWith(disposable);

        }

        public void AddNewFile(FileSignature file)
        {
            filesAvailable.AddOrUpdate(file);

        }

        public FileStream GetTemporaryFileStream(FileSignature fileSignature)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            disposable?.Dispose();
        }
    }
}
