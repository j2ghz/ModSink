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
        private string repoSelected;

        public SettingsViewModel(SettingsModel settings)
        {
            Settings = settings;
            AddRepoUrl = ReactiveCommand.CreateFromTask(async () =>
            {
                var repoUrl = await DialogCoordinator.ShowInputAsync(this, "Add new Repo",
                    "Enter the url for the new Repo you want to add.\nIt usually looks like https://example.com/someFolder/repo.bin");
                if (string.IsNullOrWhiteSpace(repoUrl)) return;
                settings.Client.GroupUrls.Edit(l => l.Add(repoUrl));
            });
            var isRepoSelected =
                this.WhenAnyValue(x => x.RepoSelected,
                    r => !string.IsNullOrWhiteSpace(r));
            RemoveRepoUrl = ReactiveCommand.Create(() => settings.Client.GroupUrls.Edit(l => l.Remove(RepoSelected)),
                isRepoSelected);
        }

        public string RepoSelected
        {
            get => repoSelected;
            set => this.RaiseAndSetIfChanged(ref repoSelected, value);
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

        public ReactiveCommand AddRepoUrl { get; }
        public ReactiveCommand RemoveRepoUrl { get; }


        public SettingsModel Settings { get; }
    }
}