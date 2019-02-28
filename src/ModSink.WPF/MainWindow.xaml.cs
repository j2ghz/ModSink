using System;
using System.Reactive.Disposables;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Anotar.Serilog;
using Humanizer;
using MahApps.Metro.Controls;
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

                if (delta > 500.Milliseconds())
                {
                    LogTo.Information("UI Freeze = {0}", delta);
                }
            };

            timer.Start();
            Application.Current.Exit += (_, __) => timer.Stop();
        }
    }
}