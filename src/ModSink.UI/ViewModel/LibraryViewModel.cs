using System;
using System.Reactive.Linq;
using Anotar.Serilog;
using DynamicData;
using DynamicData.Binding;
using ModSink.Common.Models.Repo;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ModSink.UI.ViewModel
{
    public class LibraryViewModel : ReactiveObject
    {
        public LibraryViewModel(IConnectableCache<Modpack, Guid> modpacks)
        {
            modpacks.Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(Modpacks)
                .Subscribe();
            LogTo.Verbose("Library initialized");
        }

        public ObservableCollectionExtended<Modpack> Modpacks { get; } =
            new ObservableCollectionExtended<Modpack>();

        [Reactive] public Modpack SelectedModpack { get; set; }
    }
}