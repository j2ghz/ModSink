﻿using System.Runtime.Serialization.Formatters.Binary;
using ModSink.Common;
using ModSink.WPF.Helpers;
using ModSink.WPF.Model;
using ReactiveUI;
using Splat;

namespace ModSink.WPF.ViewModel
{
    public class AppBootstrapper : ReactiveObject
    {
        public AppBootstrapper()
        {
            var modsink = new ModSinkBuilder()
                .WithDownloader(new HttpClientDownloader())
                .WithFormatter(new BinaryFormatter())
                .InDirectory(PathProvider.Downloads,PathProvider.Temp)
                .Build();
            Locator.CurrentMutable.RegisterConstant(modsink);
            var cs = modsink.Client;
            DownloadsVM = new DownloadsViewModel(cs);
            LibraryVM = new LibraryViewModel(cs);
            SettingsVM = new SettingsViewModel(new SettingsModel(cs));
        }

        public SettingsViewModel SettingsVM { get; }
        public DownloadsViewModel DownloadsVM { get; }
        public LibraryViewModel LibraryVM { get; }
    }
}