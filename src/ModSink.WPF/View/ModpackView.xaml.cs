using System.Reactive.Disposables;
using ModSink.WPF.ViewModel;
using ReactiveUI;

namespace ModSink.WPF.View
{
    public partial class ModpackView : ReactiveUserControl<ModpackViewModel>
    {
        public ModpackView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Modpack.Name, v => v.TbName.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Size, v => v.TbSize.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Modpack.Mods, v => v.LbMods.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Modpack.Selected, v => v.ChkInstall.IsChecked).DisposeWith(d);
            });
        }
    }
}