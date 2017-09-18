using Autofac;
using ModSink.WPF.Helpers;
using ModSink.WPF.ViewModel;
using ReactiveUI;
using Splat;
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

            var builder = new ContainerBuilder();

            builder.RegisterForReactiveUI(typeof(AppBootstrapper).Assembly);

            var container = builder.Build();
            Locator.Current = new AutofacDependencyResolver(container);

            this.AppBootstrapper = container.Resolve<AppBootstrapper>();
            this.DataContext = this.AppBootstrapper;

            AppBootstrapper.Router.Navigate.Execute(container.Resolve<ILibraryViewModel>());
        }

        public AppBootstrapper AppBootstrapper { get; protected set; }
    }
}