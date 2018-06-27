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
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;

namespace ModSink.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            DataContext = viewModel;

            InitializeComponent();

            this.Title = $"Modsink {typeof(App).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}";
        }
    }
}