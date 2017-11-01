using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Squirrel;
using Serilog;
using SharpRaven;
using System.Reflection;
using Autofac;
using System.Runtime.Serialization.Formatters.Binary;
using ModSink.Common.Client;
using ModSink.Core;
using System.Runtime.Serialization;
using System.Windows.Controls;
using AutofacSerilogIntegration;

namespace ModSink.WPF
{
    public partial class App : Application
    {
        private ILogger log;

        private event EventHandler<Exception> UpdateFailed;

        private string FullVersion => typeof(App).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                Helpers.ConsoleManager.Show();
            }
            Serilog.Debugging.SelfLog.Enable(Console.Error);
            SetupLogging();
            this.log = Log.ForContext<App>();
            this.log.Information("Starting ModSink ({version})", FullVersion);
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                //Do not report errors during development
                SetupSentry();
                CheckUpdates();
            }
            

            base.OnStartup(e);

            var container = BuildContainer();

            log.Information("Starting UI");
            var mw = container.Resolve<MainWindow>();
            mw.ShowDialog();
        }

        private IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterLogger();

            builder.RegisterType<BinaryFormatter>().As<IFormatter>().SingleInstance();
            builder.Register(_ => new LocalStorageService(new System.Uri(@"D:\modsink"))).AsImplementedInterfaces().SingleInstance();
            builder.RegisterAssemblyTypes(typeof(IModSink).Assembly, typeof(ModSink.Common.ModSink).Assembly).Where(t => t.Name != "LocalStorageService").AsImplementedInterfaces().SingleInstance();

            builder.RegisterAssemblyTypes(typeof(App).Assembly).Where(t => t.Name.EndsWith("ViewModel")).AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterAssemblyTypes(typeof(App).Assembly).Where(t => t.IsAssignableTo<TabItem>()).AsSelf().As<TabItem>().SingleInstance();
            builder.RegisterType<MainWindow>().AsSelf().SingleInstance();

            //TODO: Load plugins, waiting on https://stackoverflow.com/questions/46351411

            return builder.Build();
        }

        private void CheckUpdates()
        {
            var updateLog = Log.ForContext<UpdateManager>();
            var task = new Task( () =>
           {
               this.log.Information("Looking for updates");
               try
               {
                   using (var mgr = UpdateManager.GitHubUpdateManager("https://github.com/j2ghz/ModSink", prerelease: true).GetAwaiter().GetResult())
                   {
                       if (mgr.IsInstalledApp)
                       {
                           var release = mgr.UpdateApp(i => updateLog.Debug("Download progress: {progress}", i)).GetAwaiter().GetResult();
                           this.log.Debug("Latest version: {version}", release.Version);
                       }
                   }
               }
               catch (Exception e)
               {
                   updateLog.Error(e, "Exception during update checking");
                   UpdateFailed?.Invoke(null, e);
               }
               finally
               {
                   updateLog.Debug("Update check finished");
               }
           });
            task.Start();
        }

        private void SetupLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.LiterateConsole(
                        outputTemplate: "{Timestamp:HH:mm:ss} {Level:u3} [{SourceContext}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.RollingFile(
                        "../Logs/{Date}.log",
                        outputTemplate: "{Timestamp:o} [{Level:u3}] ({SourceContext}) {Message}{NewLine}{Exception}")
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .CreateLogger();
            Log.Information("Log initialized");
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Helpers.ConsoleManager.Show();
                Log.Fatal(args.ExceptionObject as Exception, nameof(AppDomain.CurrentDomain.UnhandledException));
            };
            Application.Current.DispatcherUnhandledException += (sender, args) =>
            {
                Helpers.ConsoleManager.Show();
                Log.Fatal(args.Exception, nameof(DispatcherUnhandledException));
            };
        }

        private void SetupSentry()
        {
            log.Information("Setting up exception reporting");
            var ravenClient = new RavenClient("https://410966a6c264489f8123948949c745c7:61776bfffd384fbf8c3b30c0d3ad90fa@sentry.io/189364");
            ravenClient.Release = FullVersion?.Split('+').First();
            ravenClient.ErrorOnCapture = exception =>
            {
                Log.ForContext<RavenClient>().Error(exception, "Sentry error reporting encountered an exception");
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                ravenClient.Capture(new SharpRaven.Data.SentryEvent(args.ExceptionObject as Exception));
            };
            UpdateFailed += (sender, e) =>
            {
                ravenClient.Capture(new SharpRaven.Data.SentryEvent(e));
            };
        }
    }
}