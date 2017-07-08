using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Squirrel;

namespace ModSink.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            using (var mgr = new UpdateManager("https://modsink.j2ghz.com/release"))
            {
                new Task(async () => await mgr.UpdateApp()).Start();
            }

            base.OnStartup(e);
        }
    }
}
