using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public SettingsModel Settings { get; }
    }
}
