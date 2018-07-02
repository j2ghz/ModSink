using System.Reactive.Disposables;
using System.Reflection;
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
            ViewModel = Locator.Current.GetService<AppBootstrapper>();

            InitializeComponent();

            Title =
                $"ModSink {typeof(App).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}";

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.LibraryVM, v => v.LibraryView.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.DownloadsVM, v => v.DownloadsView.ViewModel).DisposeWith(d);
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