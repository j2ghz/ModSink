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
                this.Bind(ViewModel, vm => vm.Modpack.Name, v => v.TbName.Text).DisposeWith(d);
            });
        }
    }
}