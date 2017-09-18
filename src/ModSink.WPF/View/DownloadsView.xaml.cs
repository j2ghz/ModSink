using ModSink.WPF.ViewModel;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ModSink.WPF.View
{
    public partial class DownloadsView : UserControl, IViewFor<IDownloadsViewModel>
    {
        public DownloadsView(IDownloadsViewModel viewModel)
        {
            this.ViewModel = viewModel;
            this.DataContext = this.ViewModel;
            InitializeComponent();
        }

        public IDownloadsViewModel ViewModel
        {
            get; set;
        }

        object IViewFor.ViewModel
        {
            get { return this.ViewModel; }
            set { this.ViewModel = (IDownloadsViewModel)value; }
        }
    }
}