using Microsoft.Extensions.Options;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

namespace ReplayFixer.Views
{
    /// <summary>
    /// Interaction logic for Container.xaml
    /// </summary>
    public partial class  Container : INavigationWindow
    {

        public string TitleBar { get; set; }
        public Container(INavigationService navigationService, IPageService pageService, IDialogService dialogService,IOptions<AppConfig> options)
        {
            DataContext = this;
            this.TitleBar = $"Replay Fixer {Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}";
            InitializeComponent();
            SetPageService(pageService);
            navigationService.SetNavigationControl(RootNavigation);
            dialogService.SetDialogControl(RootDialog);
            Loaded += Container_Loaded;
        }

        private void Container_Loaded(object sender, RoutedEventArgs e)
        {

            RootMainGrid.Visibility = Visibility.Collapsed;

            Task.Run(async () =>
            {
                // Remember to always include Delays and Sleeps in
                // your applications to be able to charge the client for optimizations later.
                //await Task.Delay(4000);

                await Dispatcher.InvokeAsync(() =>
                {
                    RootMainGrid.Visibility = Visibility.Visible;

                    Navigate(typeof(Pages.Dashboard));

                });

                return true;
            });
        }

        public void CloseWindow() => Close();

        public Frame GetFrame() => RootFrame;

        public INavigation GetNavigation() => RootNavigation;

        public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

        public void SetPageService(IPageService pageService) => RootNavigation.PageService = pageService;

        public void ShowWindow() => Show();

        private void RootNavigation_Navigated(INavigation sender, Wpf.Ui.Common.RoutedNavigationEventArgs e)
        {

        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}
