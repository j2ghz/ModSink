using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Anotar.Serilog;
using Humanizer;
using ModSink.Common.Models.Repo;
using Polly;

namespace ModSink.Common.Client
{
    public class FileAccessService : IFileAccessService
    {
        private readonly DirectoryInfo localDir;

        public FileAccessService(DirectoryInfo localDir)
        {
            this.localDir = localDir;
            if (!localDir.Exists)
                localDir.Create();
        }

        IEnumerable<FileSignature> IFileAccessService.FilesAvailable()
        {
            foreach (var fileInfo in localDir.EnumerateFiles()
                .Where(f => f.Name.EndsWith(".tmp"))
            )
                fileInfo.Delete();

            return localDir.EnumerateFiles()
                .Where(f => !f.Name.EndsWith(".tmp"))
                .Select(f => new FileSignature(new HashValue(f.Name), f.Length))
                .Do(file => LogTo.Verbose("File {file} has been discovered", file.Hash));
        }

        Stream IFileAccessService.Read(FileSignature fileSignature, bool temporary)
        {
            var file = GetFileInfo(fileSignature, temporary);
            return file.Open(FileMode.Create, FileAccess.Read, FileShare.Read);
        }

        Stream IFileAccessService.Write(FileSignature fileSignature, bool temporary)
        {
            var file = GetFileInfo(fileSignature, temporary);

            return Policy
                .Handle<IOException>()
                .WaitAndRetry(5, i => Math.Pow(2, i).Seconds(),
                    (exception, duration) => LogTo.Warning(exception,
                        "Opening file {file} for write failed after {duration}", file.FullName, duration))
                .Execute(() => file.Open(FileMode.Create, FileAccess.Write, FileShare.None));
        }

        public void TemporaryFinished(FileSignature fileSignature)
        {
            var temp = GetFileInfo(fileSignature, true);
            var final = GetFileInfo(fileSignature, false);
            LogTo.Verbose("Renaming file {src} to {dst}", temp, final);

            //Policy
            //    .Handle<IOException>()
            //    .WaitAndRetry(5, i => Math.Pow(2, i).Seconds(),
            //        (exception, duration) => LogTo.Warning(exception,
            //            "Moving file from {src} to {dst} has failed after {duration}", temp.FullName, final.FullName,
            //            duration))
            //    .Execute(() =>
            //    {
                    temp.MoveTo(final.FullName);
                //});
        }

        private FileInfo GetFileInfo(FileSignature fileSignature, bool temporary)
        {
            return new FileInfo(GetFileUri(fileSignature, temporary).FullName);
        }

        private string GetFileName(FileSignature fileSignature, bool temporary)
        {
            return fileSignature.Hash + (temporary ? ".tmp" : string.Empty);
        }

        private FileInfo GetFileUri(FileSignature fileSignature, bool temporary)
        {
            return localDir.ChildFile(GetFileName(fileSignature, temporary));
        }
    }
}