using System.Reflection;
using MahApps.Metro.Controls;
using ModSink.WPF.ViewModel;
using ReactiveUI;

namespace ModSink.WPF
{
    public partial class MainWindow : MetroWindow, IViewFor<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();

            Title =
                $"ModSink {typeof(App).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}";

            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.Router, v => v.ViewHost.Router);
            });
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (MainWindowViewModel) value;
        }

        public MainWindowViewModel ViewModel { get; set; }
    }
}