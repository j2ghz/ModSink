using System.Reactive.Disposables;
using ModSink.WPF.ViewModel;
using ReactiveUI;

namespace ModSink.WPF.View
{
    /// <summary>
    ///     Interaction logic for ModpackView.xaml
    /// </summary>
    public partial class ModpackView : ReactiveUserControl<ModpackViewModel>
    {
        public ModpackView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Modpack.Name, v => v.TbName.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Size, v => v.TbSize).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Modpack.Mods, v => v.TrvMods.ItemsSource).DisposeWith(d);
            });
        }
    }
}