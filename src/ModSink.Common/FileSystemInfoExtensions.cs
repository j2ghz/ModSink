using System.IO;

namespace ModSink.Common
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