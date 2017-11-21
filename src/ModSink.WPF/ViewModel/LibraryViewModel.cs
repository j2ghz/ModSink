using System;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using DynamicData.ReactiveUI;
using ModSink.Core.Client;
using ModSink.Core.Models.Repo;
using ReactiveUI;

namespace ModSink.WPF.ViewModel
{
    public class LibraryViewModel : ReactiveObject
    {
        public ObservableCollectionExtended<ModpackViewModel> Modpacks { get; } = new ObservableCollectionExtended<ModpackViewModel>();
        private ModpackViewModel selectedModpack;

        public LibraryViewModel(IClientService clientService)
        {
            ClientService = clientService;
            ClientService.Modpacks
                .Connect()
                .Transform(m=> new ModpackViewModel(m))
                .ObserveOnDispatcher()
                .Bind(Modpacks)
                .Subscribe();
        }

        public IClientService ClientService { get; }

        public ModpackViewModel SelectedModpack
        {
            get => selectedModpack;
            set => this.RaiseAndSetIfChanged(ref selectedModpack, value);
        }
    }
}