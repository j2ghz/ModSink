using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Anotar.Serilog;
using Humanizer;
using Humanizer.Bytes;
using MahApps.Metro.Controls;
using ModSink.WPF.Services;
using ModSink.WPF.ViewModel;
using ReactiveUI;
using Splat;

namespace ModSink.WPF
{
    public partial class MainWindow : MetroWindow, IViewFor<AppBootstrapper>, IEnableLogger
    {
        public MainWindow()
        {
            ViewModel = Locator.Current.GetService<AppBootstrapper>();

            InitializeComponent();

            Title =
                $"ModSink {typeof(App).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}";

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.LibraryVM, v => v.LibraryView.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.DownloadsVM, v => v.DownloadsView.ViewModel).DisposeWith(d);
            });

            StartMonitoring();
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (AppBootstrapper) value;
        }

        public AppBootstrapper ViewModel { get; set; }

        private void StartMonitoring()
        {
            var dianosticsService = new DiagnosticsService(Application.Current.MainWindow);

            var listenDisposable = Observable.Interval(5.Seconds()).Select(_ => Unit.Default)
                .SelectMany(x => dianosticsService.Memory.Take(1), (x, y) => y)
                .SelectMany(x => dianosticsService.Cpu.Take(1), (x, y) => new Tuple<Memory, int>(x, y))
                .SelectMany(x => dianosticsService.Rps.Take(1),
                    (x, y) => new Tuple<Memory, int, int>(x.Item1, x.Item2, y))
                .Subscribe(x => LogTo.Information("Heartbeat ({0}, {1}% CPU, {2}RPS)",
                    ByteSize.FromBytes(Convert.ToDouble(x.Item1.WorkingSetPrivate)).Humanize("G04"), x.Item2, x.Item3));

            Application.Current.Exit += (_, __) => listenDisposable.Dispose();

            var timer = new DispatcherTimer(DispatcherPriority.Normal)
            {
                Interval = 333.Milliseconds()
            };

            var previous = DateTime.Now;
            timer.Tick += (sender, args) =>
            {
                var current = DateTime.Now;
                var delta = current - previous;
                previous = current;

                if (delta > 500.Milliseconds()) Debug.WriteLine($"UI Freeze = {delta.Humanize(2)}");
            };

            timer.Start();
            Application.Current.Exit += (_, __) => timer.Stop();
        }
    }
}