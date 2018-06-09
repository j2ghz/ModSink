using System;
using System.Collections.Generic;
using System.Data.HashFunction.xxHash;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive;
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
using ReactiveUI;
using Serilog;
using Serilog.Debugging;
using Squirrel;

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

        private void CheckUpdates()
        {
            var updateLog = Log.ForContext<UpdateManager>();
            Task.Factory.StartNew(async () =>
            {
                log.Information("Looking for updates");
                Countly.RecordEvent("UpdateCheck");
                try
                {
                    using (var mgr =
                        await UpdateManager.GitHubUpdateManager("https://github.com/j2ghz/ModSink", prerelease: true))
                    {
                        var updates = await mgr.CheckForUpdate(false, i =>
                            updateLog.Debug("Checking for updates {progress:P0}", i));
                        var rel = updates.ReleasesToApply;
                        if (!rel.Any()) return;
                        Countly.RecordEvent("UpdateInProgress");
                        await mgr.DownloadReleases(rel, i =>
                            updateLog.Debug("Downloading updates {progress:P0}", i));
                        await mgr.ApplyReleases(updates, i =>
                            updateLog.Debug("Installing updates {progress:P0}", i));
                        mgr.CreateShortcutForThisExe();
                        await mgr.CreateUninstallerRegistryEntry();
                        log.Information("Installed version: {version}", updates.FutureReleaseEntry.Version);
                        Countly.RecordEvent("UpdateFinished");
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

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!Debugger.IsAttached)
                ConsoleManager.Show();
            SelfLog.Enable(Console.Error);
            SetupLogging();
            log.Information("Starting ModSink ({version})", FullVersion);
            if (!Debugger.IsAttached)
                CheckUpdates();

            base.OnStartup(e);

            var container = BuildContainer();

            log.Information("Starting UI");
            this.MainWindow = container.Resolve<MainWindow>();
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
            this.MainWindow.Show();
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