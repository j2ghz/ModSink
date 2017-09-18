using Autofac;
using ModSink.WPF.View;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModSink.WPF.ViewModel
{
    public class AppBootstrapper : ReactiveObject, IScreen
    {
        public AppBootstrapper()
        {
            // TODO: This is a good place to set up any other app
            // startup tasks, like setting the logging level
            LogHost.Default.Level = LogLevel.Debug;
            Splat.Serilog.Registration.Register(Serilog.Log.Logger.ForContext<Splat.ILogger>());
        }

        public RoutingState Router { get; private set; } = new RoutingState();
    }
}