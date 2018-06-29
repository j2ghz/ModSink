using System.Reactive.Disposables;
using ModSink.Core.Models.Repo;
using ReactiveUI;

namespace ModSink.WPF.View
{
    /// <summary>
    ///     Interaction logic for ModView.xaml
    /// </summary>
    public partial class ModView : ReactiveUserControl<ModEntry>
    {
        public ModView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
                {
                    this.Bind(ViewModel, vm => vm.Mod.Name, v => v.TbName.Text).DisposeWith(d);
                    this.Bind(ViewModel, vm => vm.Mod.Version, v => v.TbVersion.Text).DisposeWith(d);
                }
            );
        }
    }
}