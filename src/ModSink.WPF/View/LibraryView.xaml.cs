using System.Reactive.Disposables;
using ModSink.UI.ViewModel;
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
                this.OneWayBind(ViewModel, vm => vm.Modpacks, v => v.ModpacksItemsControl.ItemsSource).DisposeWith(d);
            });
        }
    }
}