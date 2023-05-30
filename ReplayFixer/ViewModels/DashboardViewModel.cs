using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ReplayFixer.Models.Helpers;
using ReplayFixer.Services;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;
namespace ReplayFixer.ViewModels
{
    public class DashboardViewModel : ObservableObject, INavigationAware
    {
        private readonly INavigationService _navigationService;
        private readonly ILogger<DashboardViewModel> _logger;

        private readonly MessageService _messageService;
        private readonly IDialogControl _dialogControl;
        public string ChangelogText{ get => _changelogText; set => SetProperty(ref _changelogText, value); }
        private string _changelogText= string.Empty;


        public MessageService MessageService { get { return _messageService; } }

        private ICommand _navigateCommand;
        public ICommand NavigateCommand => _navigateCommand ??= new RelayCommand<Type>(OnNavigate);

        private ICommand _openAboutDialogCommand;
        public ICommand OpenAboutDialogCommand => _openAboutDialogCommand ??= new RelayCommand<Type>(OnOpenAboutDialog);

        private void OnOpenAboutDialog(Type? obj)
        {
            var stackPanel = new StackPanel();
            stackPanel.Margin = new Thickness(5, 5, 5, 5);

            var closeButton = new Wpf.Ui.Controls.Button();
            closeButton.HorizontalAlignment = HorizontalAlignment.Right;
            closeButton.Content = "Close";
            closeButton.Click += ((sender, args) =>
            {
                _dialogControl.Hide();
            });
            stackPanel.Children.Add(closeButton);

            _dialogControl.Footer = stackPanel;
            var result = _dialogControl.Show("About", "Thanks to @Virgolandia2 for testing early stages of the app.");
        }

        private void OnNavigate(Type? parameter)
        {
            _navigationService.Navigate(parameter);
        }

        public DashboardViewModel(INavigationService navigationService,
            IDialogService dialogService,
            MessageService messageService,ILogger<DashboardViewModel> logger)
        {
            _navigationService = navigationService;
            _logger = logger;
            _messageService = messageService;
            _dialogControl = dialogService.GetDialogControl();
            this.Initialize();
        }

        private void Initialize()
        {
            try
            {
                ChangelogText = File.ReadAllText("changelog.txt");
            } catch(Exception ex)
            {
                _logger.LogError($"{HelperMethods.GetCurrentMethod()}: Error trying to read changelog file: {ex.Message}");
            }
        }
        public void OnNavigatedFrom()
        {
            _logger.LogInformation($"{typeof(DashboardViewModel)} navigated", "ReplayFixer");
        }

        public void OnNavigatedTo()
        {
            _logger.LogInformation($"{typeof(DashboardViewModel)} navigated", "ReplayFixer");
        }
    }
}
