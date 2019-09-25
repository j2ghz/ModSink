﻿using System;
using System.Reactive.Linq;
using Anotar.Serilog;
using DynamicData;
using DynamicData.Binding;
using ModSink.Common.Client;
using ModSink.Common.Models.DTO.Repo;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Modpack = ModSink.Common.Models.Client.Modpack;

namespace ModSink.UI.ViewModel
{
    public class LibraryViewModel : ReactiveObject
    {
        public LibraryViewModel(IObservableCache<Modpack, Guid> modpacks)
        {
            modpacks.Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Transform(m=>new ModpackViewModel(m))
                .Bind(Modpacks)
                .Subscribe();
            LogTo.Verbose("Library initialized");
        }

        public ObservableCollectionExtended<ModpackViewModel> Modpacks { get; } =
            new ObservableCollectionExtended<ModpackViewModel>();
    }
}