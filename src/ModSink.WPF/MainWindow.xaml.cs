using ModSink.Common.Client;
using ModSink.Core;
using ModSink.WPF.Helpers;
using ModSink.WPF.View;
using ModSink.WPF.ViewModel;
using ReactiveUI;
using Splat;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;

namespace ModSink.WPF
{
    public partial class MainWindow : MetroWindow, IViewFor<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Title = $"ModSink {typeof(App).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}";

            this.WhenActivated(d =>
                {
                    this.OneWayBind(ViewModel, vm => vm.LibraryVM, v => v.LibraryView.ViewModel).DisposeWith(d);
                });
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (MainWindowViewModel)value  ;
        }

        public MainWindowViewModel ViewModel { get; set; }
    }
}