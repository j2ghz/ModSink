using Autofac;
using ModSink.Common.Client;
using ModSink.Core;
using ModSink.WPF.Helpers;
using ModSink.WPF.ViewModel;
using ReactiveUI;
using Splat;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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

            builder.RegisterType<BinaryFormatter>().As<IFormatter>();
            builder.Register(_ => new LocalRepoManager(new System.Uri(@"D:\modsink"))).AsImplementedInterfaces();
            builder.RegisterForReactiveUI(typeof(AppBootstrapper).Assembly);
            builder.RegisterAssemblyTypes(typeof(IModSink).Assembly, typeof(ModSink.Common.ModSink).Assembly).Where(t => t.Name != "LocalRepoManager").AsImplementedInterfaces();

            var container = builder.Build();
            Locator.Current = new AutofacDependencyResolver(container);

            this.AppBootstrapper = container.Resolve<AppBootstrapper>();
            this.DataContext = this.AppBootstrapper;

            AppBootstrapper.Router.Navigate.Execute(container.Resolve<IDownloadsViewModel>());
        }

        public AppBootstrapper AppBootstrapper { get; protected set; }
    }
}