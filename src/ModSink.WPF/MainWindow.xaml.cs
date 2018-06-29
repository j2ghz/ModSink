using System;
using System.Reactive.Disposables;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using ModSink.WPF.ViewModel;
using ReactiveUI;
using Splat;

namespace ModSink.WPF
{
    public partial class MainWindow : MetroWindow, IViewFor<AppBootstrapper>, IEnableLogger
    {
        public MainWindow()
        {
            this.ViewModel = Locator.Current.GetService<AppBootstrapper>();

            InitializeComponent();

            Title =
                $"ModSink {typeof(App).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}";

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.LibraryVM, v => v.LibraryView.ViewModel).DisposeWith(d);
                this.Events().Closed.Log(this,"Closing main window").Subscribe(e => Application.Current.Shutdown());
            });
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (AppBootstrapper) value;
        }

        public AppBootstrapper ViewModel { get; set; }
    }
}