using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ReplayFixer.Services;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

namespace ReplayFixer.ViewModels
{
    public class DashboardViewModel : ObservableObject, INavigationAware
    {
        private readonly INavigationService _navigationService;
        private readonly ILogger<DashboardViewModel> _logger;

        private readonly MessageService _messageService;

        public MessageService MessageService { get { return _messageService; } }

        private ICommand _navigateCommand;
        public ICommand NavigateCommand => _navigateCommand ??= new RelayCommand<Type>(OnNavigate);
        
        private void OnNavigate(Type? parameter)
        {
            _navigationService.Navigate(parameter);
        }

        public DashboardViewModel(INavigationService navigationService, MessageService messageService,ILogger<DashboardViewModel> logger)
        {
            _navigationService = navigationService;
            _logger = logger;
            _messageService = messageService;
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
