using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DynamicData;
using ModSink.Common.Models.Repo;
using ReactiveUI;

namespace ModSink.Common.Client
{
    public class LocalFilesManager : ReactiveObject
    {
        private readonly DirectoryInfo localDir;
        private readonly SourceCache<FileSignature,HashValue> filesAvailable;
        private readonly IObservable<IChangeSet<FileSignature,HashValue>> filesToDownload;

        public LocalFilesManager(IObservable<IChangeSet<FileSignature,HashValue>> filesRequired)
        {
            filesAvailable.Edit(l =>
            {
                l.AddOrUpdate(localDir.EnumerateFiles()
                    .Select(fi => new FileSignature(new HashValue(fi.Name), fi.Length)));
            });
            filesToDownload = filesRequired.Except(filesAvailable.Connect());
            
        }

        public void AddNewFile(FileSignature file)
        {
            filesAvailable.AddOrUpdate(file);

        }

    }
}
