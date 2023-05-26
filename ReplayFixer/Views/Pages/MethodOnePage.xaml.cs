using ReplayFixer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace ReplayFixer.Views.Pages
{
    /// <summary>
    /// Interaction logic for MethodOnePage.xaml
    /// </summary>
    public partial class MethodOnePage : INavigableView<MethodOneViewModel>
    {
        public MethodOneViewModel ViewModel { get; }
        public MethodOnePage(MethodOneViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }

        private bool _initialized = false;
        private void UiPage_Loaded(object sender, RoutedEventArgs e)
        {

            if (_initialized)
                return;

            _initialized = true;

            MainGrid.Visibility = Visibility.Collapsed;
            LoadingGrid.Visibility = Visibility.Visible;

            //_taskBarService.SetState(this, TaskBarProgressState.Indeterminate);

            Task.Run(async () =>
            {
                // Remember to always include Delays and Sleeps in
                // your applications to be able to charge the client for optimizations later.
                await Dispatcher.InvokeAsync(() =>
                {
                    LoadingGrid.Visibility = Visibility.Hidden;
                    MainGrid.Visibility = Visibility.Visible;

                    //Navigate(typeof(Pages.Dashboard));

                    //_taskBarService.SetState(this, TaskBarProgressState.None);
                });

                return true;
            });
        }
    }

}
