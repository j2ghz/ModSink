using System.Reactive.Disposables;
using ModSink.WPF.ViewModel;
using ReactiveUI;

namespace ModSink.WPF.View
{
    public partial class DownloadsView : ReactiveUserControl<DownloadsViewModel>
    {
        public DownloadsView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.QueueCount, v => v.TbQueueCount.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Downloads, v => v.LvDownloads.ItemsSource).DisposeWith(d);
            });
        }
    }
}