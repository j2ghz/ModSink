using System;
using System.Reactive;
using System.Reactive.Linq;
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
            ClientService.Modpacks
                .Connect()
                .Transform(m => new ModpackViewModel(m))
                .ObserveOnDispatcher()
                .Bind(Modpacks)
                .Subscribe();

            var canInstall = this.WhenAnyValue(x => x.SelectedModpack).Select(m => m?.Modpack != null);
            Install = ReactiveCommand.CreateFromTask(
                async () =>
                {
                    log.Information("Installing modpack {modpack_name}", SelectedModpack.Modpack.Name);
                    await clientService.DownloadMissingFiles(SelectedModpack.Modpack);
                },
                canInstall);
        }

        private ILogger log => Log.ForContext<LibraryViewModel>();

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