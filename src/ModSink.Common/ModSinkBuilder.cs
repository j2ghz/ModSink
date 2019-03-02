﻿using System.IO;
using System.Runtime.Serialization;
using ModSink.Common.Client;

namespace ModSink.Common
{
    public class ModSinkBuilder
    {
        private IDownloader downloader;
        private IFormatter formatter;
        private DirectoryInfo localStorageDirectory;

        public ModSinkBuilder WithDownloader(IDownloader downloader)
        {
            this.downloader = downloader;
            return this;
        }

        public ModSinkBuilder WithFormatter(IFormatter formatter)
        {
            this.formatter = formatter;
            return this;
        }

        public ModSinkBuilder InDirectory(DirectoryInfo directory)
        {
            localStorageDirectory = directory;
            return this;
        }

        public ModSink Build()
        {
            return new ModSink(new ClientService(downloader, formatter, localStorageDirectory));
        }
    }
}