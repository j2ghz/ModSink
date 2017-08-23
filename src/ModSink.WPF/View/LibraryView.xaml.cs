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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ModSink.WPF.View
{
    /// <summary>
    /// Interaction logic for LibraryView.xaml
    /// </summary>
    public partial class LibraryView : UserControl, IViewFor<ILibraryViewModel>
    {
        public LibraryView(ILibraryViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }

        public ILibraryViewModel ViewModel
        {
            get; set;
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (ILibraryViewModel)value; }
        }
    }
}