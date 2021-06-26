using System.Reflection;
using System.Runtime.InteropServices;

namespace ModSink.UI.AvaloniaUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string Title => $"{Assembly.GetEntryAssembly().GetName().Name} {Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion} on {RuntimeInformation.FrameworkDescription}";
        public string Greeting => $"Hello World!";
    }
}
