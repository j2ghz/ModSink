using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ModSink.Core
{
    public static class FileSystemInfoExtensions
    {
        public static DirectoryInfo ChildDir(this FileSystemInfo di, string child)
        {
            var parentPath = di.FullName;
            var childPath = Path.Combine(parentPath, child);
            return new DirectoryInfo(childPath);
        }

        public static FileInfo ChildFile(this FileSystemInfo di, string child)
        {
            var parentPath = di.FullName;
            var childPath = Path.Combine(parentPath, child);
            return new FileInfo(childPath);
        }

    }
}
