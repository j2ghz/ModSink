using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Anotar.Serilog;
using Humanizer;
using ModSink.Common.Models.DTO.Repo;
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

            foreach (var file in localDir.EnumerateFiles()
                .Where(f => !f.Name.EndsWith(".tmp"))
                .Select(f => new FileSignature(new HashValue(f.Name), f.Length)))
            {
                LogTo.Verbose("File {file} has been discovered", file.Hash);
                yield return file;
            }
        }

        Stream IFileAccessService.Read(FileSignature fileSignature, bool temporary)
        {
            var file = GetFileInfo(fileSignature, temporary);
            return file.Open(FileMode.Create, FileAccess.Read, FileShare.Read);
        }

        public void TemporaryFinished(FileSignature fileSignature)
        {
            var temp = GetFileInfo(fileSignature, true);
            var final = GetFileInfo(fileSignature, false);
            LogTo.Verbose("Renaming file {src} to {dst}", temp, final);
            if (final.Exists) final.Delete();
            Policy
                .Handle<IOException>()
                .WaitAndRetry(5, i => Math.Pow(2, i).Seconds(),
                    (exception, duration) => LogTo.Warning(exception,
                        "Moving file from {src} to {dst} has failed after {duration}", temp.FullName, final.FullName,
                        duration))
                .Execute(() => { temp.MoveTo(final.FullName); });
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

        [DllImport("kernel32.dll")]
        private static extern bool CreateSymbolicLink(
            string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        public void CreateSymlink(FileSignature file, string destination)
        {
            CreateSymbolicLink(GetFileInfo(file, false).FullName, destination, SymbolicLink.File);
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

        private enum SymbolicLink
        {
            File = 0,
            Directory = 1
        }
    }
}