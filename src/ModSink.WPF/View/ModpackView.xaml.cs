using System.Reactive.Disposables;
using System.Windows.Controls.Primitives;
using ModSink.UI.ViewModel;
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
                this.BindCommand(ViewModel, vm => vm.Install, v => v.ChkInstall).DisposeWith(d);
            });
        }
    }
}