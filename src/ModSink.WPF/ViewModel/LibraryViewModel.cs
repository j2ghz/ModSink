using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModSink.WPF.ViewModel
{
    public interface ILibraryViewModel : IRoutableViewModel
    {
    }

    public class LibraryViewModel : ReactiveObject, ILibraryViewModel
    {
        public LibraryViewModel(IScreen screen)
        {
            HostScreen = screen;
        }

        public IScreen HostScreen { get; protected set; }

        public string UrlPathSegment
        {
            get { return "library"; }
        }
    }
}