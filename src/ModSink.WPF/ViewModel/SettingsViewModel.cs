using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MahApps.Metro;
using ModSink.WPF.Model;
using ReactiveUI;

namespace ModSink.WPF.ViewModel
{
    public class SettingsViewModel : ReactiveObject
    {
        public SettingsViewModel(SettingsModel settings)
        {
            Settings = settings;
        }

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


        public SettingsModel Settings { get; }
    }
}