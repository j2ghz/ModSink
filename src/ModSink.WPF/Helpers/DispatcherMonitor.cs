using System;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows.Threading;
using Anotar.Serilog;
using Humanizer;
using ReactiveUI;

namespace ModSink.WPF.Helpers
{
    public static class DispatcherMonitor
    {
        public static void Start()
        {
            var hooks = Dispatcher.CurrentDispatcher.Hooks.Events();

            hooks.OperationStarted.ObserveOn(RxApp.TaskpoolScheduler).Subscribe(e =>
            {
                var started = DateTime.Now;
                e.Operation.Events().Completed.ObserveOn(RxApp.TaskpoolScheduler).Subscribe(_ =>
                {
                    var duration = DateTime.Now - started;
                    if (duration.TotalMilliseconds > 75)
                        LogTo.Verbose("Operation took {duration}, ({priority}) {Name}", duration.Humanize(2),
                            e.Operation.Priority, e.Operation.GetType()
                                .GetProperty("Name",
                                    BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic)
                                ?.GetValue(e.Operation));
                });
            });
        }
    }
}