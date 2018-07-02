using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace ModSink.WPF.Helpers
{
    public class ExceptionEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("Exception", logEvent.Exception.Demystify()));
        }
    }
}