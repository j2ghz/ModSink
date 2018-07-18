using System;
using System.Collections.Generic;
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
using ModSink.WPF.ViewModel;
using ReactiveUI;

namespace ModSink.WPF.View
{
    public partial class DownloadView : ReactiveUserControl<DownloadViewModel>
    {
        public DownloadView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Name, v => v.TbName.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Speed, v => v.TbSpeed.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Downloaded, v => v.TbDownloaded.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Size, v => v.TbSize.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Progress, v => v.ProgressBar.Value).DisposeWith(d);

            });
        }
    }
}
