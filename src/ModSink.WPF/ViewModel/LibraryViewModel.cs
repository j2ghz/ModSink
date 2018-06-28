using System;
using System.Reactive;
using System.Reactive.Linq;
using Anotar.Serilog;
using DynamicData;
using DynamicData.Binding;
using ModSink.Core.Client;
using ModSink.Core.Models.Repo;
using ReactiveUI;

namespace ModSink.WPF.ViewModel
{
    public class LibraryViewModel : ReactiveObject
    {
        private Modpack _selectedModpack;

        public LibraryViewModel(IClientService clientService)
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
                    await clientService.DownloadMissingFiles(SelectedModpack);
                },
                canInstall);
            LogTo.Verbose("Library initialized");
        }

        public ReactiveCommand<Unit, Unit> Install { get; set; }

        public ObservableCollectionExtended<Modpack> Modpacks { get; } =
            new ObservableCollectionExtended<Modpack>();


        public IClientService ClientService { get; }

        public Modpack SelectedModpack
        {
            get => _selectedModpack;
            set => this.RaiseAndSetIfChanged(ref _selectedModpack, value);
        }
    }
}