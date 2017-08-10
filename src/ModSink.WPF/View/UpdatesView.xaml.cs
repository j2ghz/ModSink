using ModSink.WPF.ViewModel;
using Squirrel;
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
    /// <summary>
    /// Interaction logic for UpdatesView.xaml
    /// </summary>
    public partial class UpdatesView : Window
    {
        public UpdatesView()
        {
            this.ViewModel = new Updates();
            InitializeComponent();
            this.DataContext = this.ViewModel;
        }

        public ViewModel.Updates ViewModel { get; private set; }
    }
}