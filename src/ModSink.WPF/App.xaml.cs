using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using Anotar.Serilog;
using CountlySDK;
using ModSink.Common;
using ModSink.Common.Client;
using ModSink.WPF.Helpers;
using ModSink.WPF.Model;
using ModSink.WPF.ViewModel;
using ReactiveUI;
using Serilog;
using Serilog.Debugging;
using Splat;
using Splat.Serilog;

namespace ModSink.WPF
{
    public partial class App : Application
    {

        private static string FullVersion => typeof(App).GetTypeInfo().Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        private void InitializeDependencyInjection()
        {
            //TODO: FIX:
            ServicePointManager.DefaultConnectionLimit = 10;

            Locator.CurrentMutable.InitializeSplat();
            Registration.Register(Log.ForContext<Splat.ILogger>());
            Locator.CurrentMutable.InitializeReactiveUI();
            Locator.CurrentMutable.RegisterViewsForViewModels(typeof(App).Assembly);

            //TODO: Load plugins, waiting on https://stackoverflow.com/questions/46351411
        }

        private void FatalException(Exception e, Type source)
        {
            ConsoleManager.Show();
            Log.ForContext(source).Fatal(e, "{exceptionText}", e.ToStringDemystified());
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

        public App() : base()
        {
            if (!Debugger.IsAttached)
                ConsoleManager.Show();
            SelfLog.Enable(Console.Error);
            SetupLogging();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Log.Information("Starting ModSink ({version})", FullVersion);

            base.OnStartup(e);

            InitializeDependencyInjection();

            Log.Information("Starting UI");
            
            MainWindow = new MainWindow(){ViewModel = new MainWindowViewModel()};
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            MainWindow.Show();
        }

        private void SetupLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.LiterateConsole(
                    outputTemplate:
                    "{Timestamp:HH:mm:ss} {Level:u3} [{SourceContext}@{ThreadId}] {Message:lj} {Properties}{NewLine}{Exception}")
                .WriteTo.Debug()
                .WriteTo.RollingFile(
                    "./Logs/{Date}.log",
                    outputTemplate:
                    "{Timestamp:o} [{Level:u3}] ({SourceContext}) {Properties} {Message}{NewLine}{Exception}")
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .Enrich.With<ExceptionEnricher>()
                .MinimumLevel.Verbose()
                .CreateLogger();
            Log.Information("Log initialized");
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
                Log.ForContext(typeof(PresentationTraceSources)).Warning(m);
            }));
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning | SourceLevels.Error;
            Countly.UserDetails.Username = Environment.UserName;
            Countly.UserDetails.Organization = Environment.MachineName;
            Countly.StartSession("https://countly.j2ghz.com", "54c6bf3a77021fadb7bd5b2a66490b465d4382ac", FullVersion);
            
        }
    }
}