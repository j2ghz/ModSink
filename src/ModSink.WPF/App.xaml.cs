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

namespace ModSink.WPF
{
    public partial class App : Application
    {
        private ILogger log;

        protected override void OnStartup(StartupEventArgs e)
        {
            Helpers.ConsoleManager.Show();
            Serilog.Debugging.SelfLog.Enable(Console.Error);
            SetupLogging();
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Helpers.ConsoleManager.Hide();
            }
            this.log = Log.ForContext<App>();
            log.Information("Starting ModSink ({version})", System.Reflection.Assembly.GetEntryAssembly().GetName().Version);
            SetupSentry();
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                CheckUpdates();
            }
            base.OnStartup(e);
        }

        private void SetupSentry()
        {
            var ravenClient = new RavenClient("https://410966a6c264489f8123948949c745c7:61776bfffd384fbf8c3b30c0d3ad90fa@sentry.io/189364");
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                ravenClient.Capture(new SharpRaven.Data.SentryEvent(args.ExceptionObject as Exception));
            };
            ravenClient.ErrorOnCapture = exception =>
            {
                Log.ForContext<RavenClient>().Error(exception, "Sentry error reporting encountered an exception");
            };
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

        private void CheckUpdates()
        {
            log.Information("Looking for updates");
            using (var mgr = new UpdateManager("https://modsink.j2ghz.com/release"))
            {
                log.Information("Currently installed: {version}", mgr.CurrentlyInstalledVersion().ToString());
                new Task(async () => await mgr.UpdateApp(p => log.Verbose("Checking updates {progress}", p)).ConfigureAwait(false)).Start();
            }
        }
    }
}
