using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;
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
                var repoUrl = await DialogCoordinator.ShowInputAsync(this, "Add new Group",
                    "Enter the url for the new Repo you want to add.\nIt usually looks like https://example.com/someFolder/group.bin");
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


        public IDialogCoordinator DialogCoordinator { private get; set; }

        public ICollection<Accent> Accents { get; } = ThemeManager.Accents.ToList();

        public Accent AccentSelected
        {
            get => ThemeManager.DetectAppStyle(Application.Current).Item2;
            set
            {
                this.RaisePropertyChanging();
                ThemeManager.ChangeAppStyle(Application.Current, value,
                    ThemeManager.DetectAppStyle(Application.Current).Item1);
                this.RaisePropertyChanged();
            }
        }

        public ICollection<AppTheme> Themes { get; } = ThemeManager.AppThemes.ToList();


        public AppTheme ThemeSelected
        {
            get => ThemeManager.DetectAppStyle(Application.Current).Item1;
            set
            {
                this.RaisePropertyChanging();
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.DetectAppStyle(Application.Current).Item2,
                    value);
                this.RaisePropertyChanged();
            }
        }

        public ReactiveCommand AddGroup { get; }
        public ReactiveCommand RemoveGroup { get; }


        public SettingsModel Settings { get; }
    }
}