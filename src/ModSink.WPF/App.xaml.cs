using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Autofac;
using CountlySDK;
using ModSink.Common.Client;
using ModSink.Core;
using ModSink.WPF.Helpers;
using Serilog;
using Serilog.Debugging;

namespace ModSink.WPF
{
    public partial class App : Application
    {
        private ILogger log;

        private static string FullVersion => typeof(App).GetTypeInfo().Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        private IContainer BuildContainer()
        {
            //TODO: FIX:
            ServicePointManager.DefaultConnectionLimit = 10;

            var builder = new ContainerBuilder();

            builder.RegisterType<BinaryFormatter>().As<IFormatter>().SingleInstance();
            builder.Register(_ =>
                    new LocalStorageService(new Uri(
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ModSink_Data"))))
                .AsImplementedInterfaces()
                .SingleInstance();
            builder.RegisterAssemblyTypes(typeof(IModSink).Assembly, typeof(Common.ModSink).Assembly)
                .Where(t => t.Name != "LocalStorageService").AsImplementedInterfaces().SingleInstance();

            builder.RegisterAssemblyTypes(typeof(App).Assembly).Where(t => t.Name.EndsWith("Model"))
                .AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterAssemblyTypes(typeof(App).Assembly).Where(t => t.Name.EndsWith("ViewModel"))
                .AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterAssemblyTypes(typeof(App).Assembly).Where(t => t.IsAssignableTo<TabItem>()).AsSelf()
                .As<TabItem>().SingleInstance();
            builder.RegisterType<MainWindow>().AsSelf().SingleInstance();

            //TODO: Load plugins, waiting on https://stackoverflow.com/questions/46351411

            return builder.Build();
        }

        private void FatalException(Exception e, Type source)
        {
            ConsoleManager.Show();
            log.ForContext(source).Fatal(e, "{exceptionText}", e.ToStringDemystified());
            Countly.RecordException(e.Message, e.ToStringDemystified(), null, true);
            if (Debugger.IsAttached == false)
            {
                Console.WriteLine(WPF.Properties.Resources.FatalExceptionPressAnyKeyToContinue);
                Console.ReadKey();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Countly.EndSession().GetAwaiter().GetResult();
            base.OnExit(e);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!Debugger.IsAttached)
                ConsoleManager.Show();
            SelfLog.Enable(Console.Error);
            SetupLogging();
            log.Information("Starting ModSink ({version})", FullVersion);

            base.OnStartup(e);

            var container = BuildContainer();

            log.Information("Starting UI");
            MainWindow = container.Resolve<MainWindow>();
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            MainWindow.Show();
        }

        private void SetupLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.LiterateConsole(
                    outputTemplate:
                    "{Timestamp:HH:mm:ss} {Level:u3} {ThreadId} [{SourceContext}] {Properties} {Message:lj}{NewLine}{Exception}")
                .WriteTo.RollingFile(
                    "../Logs/{Date}.log",
                    outputTemplate:
                    "{Timestamp:o} [{Level:u3}] ({SourceContext}) {Properties} {Message}{NewLine}{Exception}")
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .Enrich.With<ExceptionEnricher>()
                .MinimumLevel.Verbose()
                .CreateLogger();
            log = Log.ForContext<App>();
            log.Information("Log initialized");
            if (!Debugger.IsAttached)
            {
                AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                {
                    FatalException(args.ExceptionObject as Exception,
                        sender.GetType());
                };
                Current.DispatcherUnhandledException += (sender, args) =>
                {
                    FatalException(args.Exception, sender.GetType());
                };
            }

            PresentationTraceSources.Refresh();
            PresentationTraceSources.DataBindingSource.Listeners.Add(new RelayTraceListener(m =>
            {
                log.ForContext(typeof(PresentationTraceSources)).Warning(m);
            }));
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning | SourceLevels.Error;
            Countly.StartSession("https://countly.j2ghz.com", "54c6bf3a77021fadb7bd5b2a66490b465d4382ac", FullVersion);
            Countly.UserDetails.Username = Environment.UserName;
        }
    }
}