using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ModSink.Core.Models.Repo;
using ModSink.WPF.ViewModel;
using ReactiveUI;

namespace ModSink.WPF.View
{
    public partial class LibraryView : ReactiveUserControl<LibraryViewModel>
    {
        public LibraryView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Modpacks, v => v.lvModpacks.ItemsSource).DisposeWith(d);
                Disposable.Create(Debugger.Break).DisposeWith(d);
                    
            });
        }
    }
}
