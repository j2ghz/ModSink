using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Autofac;
using ModSink.Common.Client;
using ModSink.Core;
using ModSink.WPF.Helpers;
using Serilog;
using Serilog.Debugging;
using Serilog.Sinks.Sentry;
using Squirrel;

namespace ModSink.WPF
{
    public partial class App : Application
    {
        private ILogger log;

        private static string FullVersion => typeof(App).GetTypeInfo().Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!Debugger.IsAttached)
                ConsoleManager.Show();
            SelfLog.Enable(Console.Error);
            SetupLogging();
            log.Information("Starting ModSink ({version})", FullVersion);
#if !DEBUG
            if (!Debugger.IsAttached)
                CheckUpdates();
#endif

            base.OnStartup(e);

            var container = BuildContainer();

            log.Information("Starting UI");
            var mw = container.Resolve<MainWindow>();
            mw.Show();
        }

        private IContainer BuildContainer()
        {
            //TODO: FIX:
            ServicePointManager.DefaultConnectionLimit = 10;

            var builder = new ContainerBuilder();

            builder.RegisterType<BinaryFormatter>().As<IFormatter>().SingleInstance();
            builder.Register(_ => new LocalStorageService(new Uri(@"D:\modsink\"))).AsImplementedInterfaces()
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

        private void CheckUpdates()
        {
            var updateLog = Log.ForContext<UpdateManager>();
            Task.Factory.StartNew(async () =>
            {
                log.Information("Looking for updates");
                try
                {
                    using (var mgr =
                        await UpdateManager.GitHubUpdateManager("https://github.com/j2ghz/ModSink", prerelease: true))
                    {
                        var release = await mgr.UpdateApp(i =>
                            updateLog.Debug("Downloading file, progress: {progress}", i));
                        log.Information("Latest version: {version}", release.Version);
                    }
                }
                catch (Exception e)
                {
                    updateLog.Error(e, "Exception during update checking");
                }
                finally
                {
                    updateLog.Debug("Update check finished");
                }
            });
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
                .WriteTo.Sentry(
                    "https://6e3a1e08759944bb932434095137f63b:ab9ff9be2ec74518bcb0c1d860d98cbe@sentry.j2ghz.com/2",
                    FullVersion?.Substring(0, 64))
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .MinimumLevel.Verbose()
                .CreateLogger();
            log = Log.ForContext<App>();
            log.Information("Log initialized");
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                ConsoleManager.Show();
                log.ForContext(sender.GetType()).Fatal(args.ExceptionObject as Exception, "{exception}",
                    nameof(AppDomain.CurrentDomain.UnhandledException));
            };
            Current.DispatcherUnhandledException += (sender, args) =>
            {
                ConsoleManager.Show();
                log.ForContext(sender.GetType()).Fatal(args.Exception, "{exception}",
                    nameof(DispatcherUnhandledException));
            };
            //AppDomain.CurrentDomain.FirstChanceException += (sender, args) =>
            //{
            //    log.ForContext(sender.GetType()).Verbose(args.Exception, "FirstChanceException");
            //};
            PresentationTraceSources.Refresh();
            PresentationTraceSources.DataBindingSource.Listeners.Add(new RelayTraceListener(m =>
            {
                log.ForContext(typeof(PresentationTraceSources)).Error(m);
            }));
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning | SourceLevels.Error;

        }
    }
}