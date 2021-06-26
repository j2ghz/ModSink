using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Concurrency;
using System.Threading;
using Anotar.Serilog;
using Avalonia;
using Avalonia.Logging;
using Avalonia.ReactiveUI;
using Serilog;
using Serilog.Debugging;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using Splat;
using Splat.Serilog;
using ILogger = Serilog.ILogger;

namespace ModSink.UI.AvaloniaUI
{
    internal class Program
    {
        public static Stopwatch StartTime = Stopwatch.StartNew();

        // AvaloniaUI configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp(ILogger logger)
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI();

        // Initialization code. Don't use any AvaloniaUI, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static int Main(string[] args)
        {
            try
            {
                Thread.CurrentThread.Name = "Main Thread";
                SelfLog.Enable(Console.Error);
                Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .Enrich.WithAssemblyName()
                    .Enrich.WithAssemblyInformationalVersion()
                    .Enrich.WithDemystifiedStackTraces()
                    .Enrich.WithEnvironmentUserName()
                    .Enrich.WithThreadId()
                    .Enrich.WithThreadName()
                    .Enrich.WithExceptionDetails()
                    .MinimumLevel.Verbose()
                    .MinimumLevel.Override("Avalonia.Application", Serilog.Events.LogEventLevel.Warning)
                    .WriteTo.Async(
                        c => c.File(new CompactJsonFormatter(),
                            Path.Combine(Path.GetTempPath(), nameof(ModSink) + "Log", "log-.clef"), buffered: true,
                            rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true), blockWhenFull: true)
#if DEBUG
                    .WriteTo.Debug()
#endif
                    .WriteTo.Seq("https://seq.j2ghz.eu", apiKey: "mmPOEH67IfZoFAXwHCce",
                        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
                    .CreateLogger();

                Logger.Sink = new SerilogSink(Log.Logger);
                Locator.CurrentMutable.UseSerilogFullLogger(Log.Logger);

                if (!Debugger.IsAttached)
                {
                    NewThreadScheduler.Default.Catch((Exception e) =>
                    {
                        LogTo.Error(e, "Update failed");
                        return true;
                    }).Schedule(() => Update());
                }

                return BuildAvaloniaApp(Log.Logger).StartWithClassicDesktopLifetime(args);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static void Update()
        {
            var updateExe = "../Update.exe";
            var UpdateUrl = "https://a3.417rct.org/modsink/refs/heads/develop/";
            if (File.Exists(updateExe))
            {
                var pi = new ProcessStartInfo(updateExe, $"--update={UpdateUrl}") { RedirectStandardOutput = true };
                var p = new Process();
                pi.UseShellExecute = false;
                p.StartInfo = pi;
                p.OutputDataReceived += (s, e) =>
                {
                    LogTo.Information("Updater output: {Data}", e.Data);
                };
                if (p.Start())
                {
                    p.BeginOutputReadLine();
                    p.WaitForExit();
                }
                else
                {
                    LogTo.Warning("Failed to start update process at {Path}", Path.GetFullPath(updateExe));
                }
            }
            else
            {
                LogTo.Warning("Update exe not found at {Path}", Path.GetFullPath(updateExe));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "Serilog004:Constant MessageTemplate verifier", Justification = "Just forwarding")]
        private class SerilogSink : ILogSink
        {
            private readonly ILogger log;

            public SerilogSink(ILogger log)
            {
                this.log = log.ForContext<Avalonia.Application>();
            }

            public bool IsEnabled(LogEventLevel level, string area) => log.IsEnabled(ToSerilog(level));
            public void Log(LogEventLevel level, string area, object source, string messageTemplate) => log.ForContext("Area", area).ForContext(source.GetType()).Write(ToSerilog(level), messageTemplate);
            public void Log<T0>(LogEventLevel level, string area, object source, string messageTemplate, T0 propertyValue0) => log.ForContext("Area", area).ForContext(source.GetType()).Write(ToSerilog(level), messageTemplate, propertyValue0);
            public void Log<T0, T1>(LogEventLevel level, string area, object source, string messageTemplate, T0 propertyValue0, T1 propertyValue1) => log.ForContext("Area", area).ForContext(source.GetType()).Write(ToSerilog(level), messageTemplate, propertyValue0, propertyValue1);
            public void Log<T0, T1, T2>(LogEventLevel level, string area, object source, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2) => log.ForContext("Area", area).ForContext(source.GetType()).Write(ToSerilog(level), messageTemplate, propertyValue0, propertyValue1, propertyValue2);
            public void Log(LogEventLevel level, string area, object source, string messageTemplate, params object[] propertyValues) => log.ForContext("Area", area).ForContext(source.GetType()).Write(ToSerilog(level), messageTemplate, propertyValues);

            private Serilog.Events.LogEventLevel ToSerilog(LogEventLevel logEventLevel)
            {
                return logEventLevel switch
                {
                    LogEventLevel.Verbose => Serilog.Events.LogEventLevel.Verbose,
                    LogEventLevel.Debug => Serilog.Events.LogEventLevel.Debug,
                    LogEventLevel.Information => Serilog.Events.LogEventLevel.Information,
                    LogEventLevel.Warning => Serilog.Events.LogEventLevel.Warning,
                    LogEventLevel.Error => Serilog.Events.LogEventLevel.Error,
                    LogEventLevel.Fatal => Serilog.Events.LogEventLevel.Fatal,
                };
            }
        }
    }
}
