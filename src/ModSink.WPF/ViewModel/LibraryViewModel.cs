using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Anotar.Serilog;
using DynamicData;
using DynamicData.Binding;
using ModSink.Core.Client;
using ReactiveUI;
using Serilog;

namespace ModSink.WPF.ViewModel
{
    public class LibraryViewModel : ReactiveObject
    {
        private ModpackViewModel selectedModpack;

        public LibraryViewModel(IClientService clientService)
        {
            ClientService = clientService;
            ClientService.Repos
                .Connect()
                .TransformMany(r => r.Modpacks.Select(m=>new ModpackViewModel(m,r)))
                .ObserveOnDispatcher()
                .Bind(Modpacks)
                .Subscribe();

            var canInstall = this.WhenAnyValue(x => x.SelectedModpack).Select(m => m?.Modpack != null);
            Install = ReactiveCommand.CreateFromTask(
                async () =>
                {
                    LogTo.Information("Installing modpack {modpack_name}", SelectedModpack.Modpack.Name);
                    await clientService.DownloadMissingFiles(SelectedModpack.Modpack);
                },
                canInstall);
            LogTo.Verbose("Library initialized");
        }

        public ReactiveCommand<Unit, Unit> Install { get; set; }

        public ObservableCollectionExtended<ModpackViewModel> Modpacks { get; } =
            new ObservableCollectionExtended<ModpackViewModel>();


        public IClientService ClientService { get; }

        public ModpackViewModel SelectedModpack
        {
            get => selectedModpack;
            set => this.RaiseAndSetIfChanged(ref selectedModpack, value);
        }
    }
}