using System;
using System.Windows.Controls;
using MahApps.Metro.Controls.Dialogs;
using ModSink.WPF.ViewModel;

namespace ModSink.WPF.View
{
    /// <summary>
    ///     Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            DataContextChanged += (sender, args) =>
            {
                if (DataContext is SettingsViewModel d)
                    d.DialogCoordinator = DialogCoordinator.Instance;
            };
        }
    }
}