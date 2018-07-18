using System;
using System.IO;
using ModSink.Common;

namespace ModSink.WPF.Helpers
{
    public static class PathProvider
    {
        public static DirectoryInfo AppData => new DirectoryInfo(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
                Environment.SpecialFolderOption.Create), nameof(ModSink)));

        public static DirectoryInfo Downloads => AppData.ChildDir("Downloads");
        public static DirectoryInfo Logs => AppData.ChildDir("Logs");

        public static void EnsureFoldersExist()
        {
            if (!AppData.Exists) AppData.Create();
            if (!Downloads.Exists) Downloads.Create();
        }
    }
}