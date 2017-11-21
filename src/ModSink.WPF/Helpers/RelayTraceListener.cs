using System;
using System.Diagnostics;

namespace ModSink.WPF.Helpers
{
    public class RelayTraceListener : TraceListener
    {
        private readonly Action<string> submit;
        private string cache = string.Empty;

        public RelayTraceListener(Action<string> submit)
        {
            this.submit = submit;
        }

        public override void Write(string message)
        {
            cache += message;
        }

        public override void WriteLine(string message)
        {
            submit(cache + message + Environment.NewLine);
            cache = string.Empty;
        }
    }
}