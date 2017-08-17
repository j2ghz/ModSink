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
                //Checking for updates when not installed wit Squirrel will fail
                CheckUpdates();
            }
            LoadPlugins();
            log.Information("Starting UI");
            base.OnStartup(e);
        }

        private void CheckUpdates()
        {
            var task = new Task(async () =>
            {
                this.log.Information("Looking for updates");
                try
                {
                    using (var mgr = await UpdateManager.GitHubUpdateManager("https://github.com/j2ghz/ModSink", prerelease: true))
                    {
                        if (mgr.IsInstalledApp)
                        {
                            var release = await mgr.UpdateApp(i => Log.ForContext<UpdateManager>().Debug("Update progress: {progress}", i));
                            this.log.Debug("New version: {version}", release);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.ForContext<UpdateManager>().Error(e, "Exception during update checking");
                    UpdateFailed(this, e);
                }
            });
            task.Start();
        }

        private void LoadPlugins()
        {
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