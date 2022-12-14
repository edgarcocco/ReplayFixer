using ReplayFixer.ViewModels;
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
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Controls;

namespace ReplayFixer.Views.Pages
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : INavigableView<DashboardViewModel>
    {
        public DashboardViewModel ViewModel { get; }
        public Dashboard(DashboardViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }

        
    }
}
