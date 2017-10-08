using ModSink.Core.Client;
using ModSink.Core.Models.Repo;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using DynamicData.ReactiveUI;

namespace ModSink.WPF.ViewModel
{
    public class LibraryViewModel : ReactiveObject
    {
        private readonly ReactiveList<Modpack> modpacks = new ReactiveList<Modpack>();
        private Modpack selectedModpack;

        public LibraryViewModel(IClientService clientService)
        {
            this.ClientService = clientService;
            this.ClientService.Modpacks
                .Connect()
                .Bind(this.modpacks)
                .Subscribe();
        }

        public IClientService ClientService { get; }
        public IReadOnlyReactiveList<Modpack> Modpacks => this.modpacks;

        public Modpack SelectedModpack
        {
            get { return this.selectedModpack; }
            set { this.RaiseAndSetIfChanged(ref this.selectedModpack, value); }
        }
    }
}