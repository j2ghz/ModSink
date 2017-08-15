using ModSink.WPF.ViewModel;
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

            AppBootstrapper = new AppBootstrapper();
            DataContext = AppBootstrapper;
        }

        public AppBootstrapper AppBootstrapper { get; protected set; }
    }
}