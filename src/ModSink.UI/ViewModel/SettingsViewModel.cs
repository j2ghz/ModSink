using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Windows;
using ModSink.WPF.Model;
using ReactiveUI;

namespace ModSink.WPF.ViewModel
{
    public class SettingsViewModel : ReactiveObject
    {
        private string groupSelected;

        public SettingsViewModel(SettingsModel settings)
        {
            Settings = settings;
            AddGroup = ReactiveCommand.CreateFromTask(async () =>
            {
                var repoUrl = "";//await DialogCoordinator.ShowInputAsync(this, "Add new Group", "Enter the url for the new Repo you want to add.\nIt usually looks like https://example.com/someFolder/group.bin");
                if (string.IsNullOrWhiteSpace(repoUrl)) return;
                settings.Client.GroupUrls.Edit(l => l.AddOrUpdate(repoUrl));
            });
            var isRepoSelected =
                this.WhenAnyValue<SettingsViewModel, bool, string>(x => x.GroupSelected,
                    r => !string.IsNullOrWhiteSpace(r));
            RemoveGroup = ReactiveCommand.Create(() => settings.Client.GroupUrls.Edit(l => l.Remove(GroupSelected)),
                isRepoSelected);
        }

        public string GroupSelected
        {
            get => groupSelected;
            set => this.RaiseAndSetIfChanged(ref groupSelected, value);
        }

        public ReactiveCommand<Unit, Unit> AddGroup { get; }
        public ReactiveCommand<Unit, Unit> RemoveGroup { get; }


        public SettingsModel Settings { get; }
    }
}