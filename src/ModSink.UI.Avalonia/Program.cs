using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;

using Serilog;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;

using Splat;
using Splat.Serilog;

namespace ModSink.UI.Avalonia
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static int Main(string[] args)
        {
            try
            {
                Serilog.Debugging.SelfLog.Enable(Console.Error);
                Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .Enrich.WithAssemblyName()
                    .Enrich.WithAssemblyInformationalVersion()
                    .Enrich.WithDemystifiedStackTraces()
                    .Enrich.WithEnvironmentUserName()
                    .Enrich.WithThreadId()
                    .Enrich.WithExceptionDetails()
                    .MinimumLevel.Verbose()
                    .WriteTo.Async(c => c.File(new CompactJsonFormatter(), Path.Combine(Path.GetTempPath(), nameof(ModSink) + "Log", "log-.clef"), buffered: true, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true), blockWhenFull: true)
#if DEBUG
                    .WriteTo.Debug()
#endif
                    .WriteTo.Seq("http://192.168.0.2:5341", apiKey: "mmPOEH67IfZoFAXwHCce")
                    .CreateLogger();
                SerilogLogger.Initialize(Log.Logger);

                Update().ContinueWith(t => Log.Error(t.Exception, "Update failed"), TaskContinuationOptions.OnlyOnFaulted);

                return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            finally
            {
                Log.CloseAndFlush();
            }

        }

        public static async Task Update()
        {
            var updateExe = "../Update.exe";
            var UpdateUrl = "https://a3.417rct.org/modsink/refs/heads/develop/";
            if (File.Exists(updateExe))
            {

                var pi = new ProcessStartInfo(updateExe, $"--update={UpdateUrl}");
                pi.RedirectStandardOutput = true;
                var p = new Process();
                pi.UseShellExecute = false;
                p.StartInfo = pi;
                p.OutputDataReceived += (s, e) =>
                {
                    Log.Information("Updater output: {Data}", e.Data);
                };
                p.Start();
                p.BeginOutputReadLine();
                p.WaitForExit();

            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI()
                .AfterSetup(_ =>
                {
                    Locator.CurrentMutable.UseSerilogFullLogger();
                });
    }
}
