using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Concurrency;
using System.Threading;
using Anotar.Serilog;
using Avalonia;
using Avalonia.ReactiveUI;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using Splat;
using Splat.Serilog;
using ILogger = Serilog.ILogger;

namespace ModSink.UI.Avalonia
{
    internal class Program
    {
        public static DateTimeOffset StartTime = DateTimeOffset.Now;

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp(ILogger logger)
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI()
                .AfterSetup(_ =>
                {
                    Locator.CurrentMutable.UseSerilogFullLogger(logger);
                });

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static int Main(string[] args)
        {
            try
            {
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
                    .MinimumLevel.Override("Avalonia.Application", LogEventLevel.Warning)
                    .WriteTo.Async(
                        c => c.File(new CompactJsonFormatter(),
                            Path.Combine(Path.GetTempPath(), nameof(ModSink) + "Log", "log-.clef"), buffered: true,
                            rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true), blockWhenFull: true)
#if DEBUG
                    .WriteTo.Debug()
#endif
                    .WriteTo.Seq("https://seq.j2ghz.eu", apiKey: "mmPOEH67IfZoFAXwHCce",
                        restrictedToMinimumLevel: LogEventLevel.Information)
                    .CreateLogger();

                // TODO: forward avalonia logs to serilog
                //SerilogLogger.Initialize(Log.Logger.ForContext<global::Avalonia.Application>());

                if (!Debugger.IsAttached)
                {
                    NewThreadScheduler.Default.Catch((Exception e) =>
                    {
                        LogTo.Error(e, "Update failed");
                        return true;
                    }).Schedule(() => Update());
                }


                Thread.CurrentThread.Name = "Main Thread";

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
                p.Start();
                p.BeginOutputReadLine();
                p.WaitForExit();
            }
            else
            {
                LogTo.Information("Update exe not found at {path}", Path.GetFullPath(updateExe));
            }
        }
    }
}
