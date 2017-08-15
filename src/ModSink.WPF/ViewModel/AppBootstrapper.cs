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
        public AppBootstrapper(IMutableDependencyResolver dependencyResolver = null, RoutingState testRouter = null)
        {
            Router = testRouter ?? new RoutingState();
            dependencyResolver = dependencyResolver ?? Locator.CurrentMutable;

            // Bind
            RegisterParts(dependencyResolver);

            // TODO: This is a good place to set up any other app
            // startup tasks, like setting the logging level
            LogHost.Default.Level = LogLevel.Debug;
            Splat.Serilog.Registration.Register(Serilog.Log.Logger.ForContext<Splat.ILogger>());

            // Navigate to the opening page of the application
            Router.Navigate.Execute(new LibraryViewModel(this));
        }

        public RoutingState Router { get; private set; }

        private void RegisterParts(IMutableDependencyResolver dependencyResolver)
        {
            dependencyResolver.RegisterConstant(this, typeof(IScreen));

            dependencyResolver.Register(() => new LibraryView(), typeof(IViewFor<LibraryViewModel>));
        }
    }
}