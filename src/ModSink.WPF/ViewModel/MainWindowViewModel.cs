using System;
using System.Runtime.Serialization.Formatters.Binary;
using ModSink.Common;
using ModSink.Common.Client;
using ModSink.WPF.Helpers;
using ModSink.WPF.Model;
using ReactiveUI;

namespace ModSink.WPF.ViewModel
{
    public class MainWindowViewModel : ReactiveObject, IScreen
    {
        public MainWindowViewModel()
        {
            var modsink = new ModSinkBuilder()
                .WithDownloader(new HttpClientDownloader())
                .WithFormatter(new BinaryFormatter())
                .InDirectory(PathProvider.Downloads)
                .Build();

            Router
                .NavigateAndReset
                .Execute(new LibraryViewModel(modsink.Client))
                .Subscribe();
        }
        public RoutingState Router { get; } = new RoutingState();
    }
}