using System;
using System.Reactive;
using System.Reactive.Linq;
using Anotar.Serilog;
using DynamicData;
using DynamicData.Binding;
using ModSink.Common.Client;
using ModSink.Common.Models.Repo;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ModSink.WPF.ViewModel
{
    public class LibraryViewModel : ReactiveObject
    {
        public LibraryViewModel(ClientService clientService)
        {
            ClientService = clientService;
            ClientService.Repos
                .Connect()
                .TransformMany(r => r.Modpacks)
                .ObserveOnDispatcher()
                .Bind(Modpacks)
                .Subscribe();

            var canInstall = this.WhenAnyValue(x => x.SelectedModpack).Select(m => m != null);
            Install = ReactiveCommand.CreateFromTask(
                async () =>
                {
                    LogTo.Information("Installing modpack {modpack_name}", SelectedModpack.Name);
                    await clientService.ScheduleMissingFilesDownload(SelectedModpack);
                },
                canInstall);
            LogTo.Verbose("Library initialized");
        }

        [Reactive] public ReactiveCommand<Unit, Unit> Install { get; set; }

        [Reactive]
        public ObservableCollectionExtended<Modpack> Modpacks { get; set; } =
            new ObservableCollectionExtended<Modpack>();


        [Reactive] public ClientService ClientService { get; set; }

        [Reactive] public Modpack SelectedModpack { get; set; }
    }
}