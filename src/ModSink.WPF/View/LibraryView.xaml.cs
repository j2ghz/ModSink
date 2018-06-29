using System.Reactive.Disposables;
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
                this.OneWayBind(ViewModel, vm => vm.Modpacks, v => v.LbModpacks.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedModpack, v => v.LbModpacks.SelectedItem);
                this.OneWayBind(ViewModel, vm => vm.SelectedModpack, v => v.VModpack.ViewModel,
                    m => m != null ? new ModpackViewModel(m) : null);
            });
        }
    }
}