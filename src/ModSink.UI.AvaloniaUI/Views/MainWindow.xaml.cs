using System;
using Anotar.Serilog;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ModSink.UI.AvaloniaUI.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

        protected override void OnOpened(EventArgs e) => LogTo.Information(
            "{Name} opened, time since startup: {SinceStartup}", nameof(MainWindow),
            Program.StartTime.Elapsed);
    }
}
