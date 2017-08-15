using ModSink.WPF.ViewModel;
using System.Reflection;
using System.Windows;

namespace ModSink.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Title = $"Modsink {typeof(App).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}";

            AppBootstrapper = new AppBootstrapper();
            DataContext = AppBootstrapper;
        }

        public AppBootstrapper AppBootstrapper { get; protected set; }
    }
}