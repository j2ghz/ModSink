using Autofac;
using ModSink.WPF.ViewModel;
using ReactiveUI.Autofac;
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
            RxAppAutofacExtension.UseAutofacDependencyResolver(container);

            AppBootstrapper = container.Resolve<AppBootstrapper>();
            var mainView = container.Resolve<LibraryViewModel>();
            AppBootstrapper.Router.Navigate.Execute(mainView);
            DataContext = AppBootstrapper;
        }

        public AppBootstrapper AppBootstrapper { get; protected set; }
    }
}