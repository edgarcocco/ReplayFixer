using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReplayFixer.Services;
using ReplayFixer.Services.Contracts;
using ReplayFixer.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Demo.Services;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace ReplayFixer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App 
    {

        private IHost _host;
        public App()
        {
            _host = new HostBuilder()
                    .ConfigureAppConfiguration((context, configurationBuilder) =>
                    {
                        configurationBuilder.SetBasePath(context.HostingEnvironment.ContentRootPath);
                        configurationBuilder.AddJsonFile("appsettings.json", optional: true);
                    })
                    .ConfigureServices((context, services) =>
                    {

                        services.AddOptions();
                        services.AddLocalization(options => {
                            options.ResourcesPath = "Resources";
                        });
                        services.AddLogging();
                        services.Configure<AppConfig>(context.Configuration.GetSection("AppConfig"));
                        services.AddHostedService<ApplicationHostService>();


                        services.AddTransient<MessageService>();
                        
                        services.AddSingleton<IPageService, PageService>();
                        services.AddSingleton<IThemeService, ThemeService>();
                        services.AddSingleton<INavigationService, NavigationService>();
                        //services.AddSingleton<INotifyIconService, NotifyIconService>();

                        services.AddScoped(typeof(IDelimitedFileService<>), typeof(DelimitedFileService<>));
                        services.AddScoped(typeof(IFileService<>), typeof(FileService<>));


                        services.AddScoped<INavigationWindow, Views.Container>();

                        services.AddScoped<Views.Pages.Dashboard>();
                        services.AddScoped<ViewModels.DashboardViewModel>();

                        services.AddScoped<Views.Pages.MethodOnePage>();
                        services.AddScoped<ViewModels.MethodOneViewModel>();

                        services.AddScoped<Views.Pages.SettingsPage>();
                    })
                    .ConfigureLogging((context, logging) =>
                    {
                        logging.AddConfiguration(context.Configuration.GetSection("Logging"));
                        logging.AddConsole();
                        logging.AddFile($"{AppDomain.CurrentDomain.BaseDirectory}/debug.log");
#if DEBUG
                        logging.AddDebug();
#endif
                    }).Build();

        }

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            await _host.StartAsync();
            var container = _host.Services.GetService<Container>();
            container?.Show();
        }

        private async void Application_Exit(object sender, ExitEventArgs e)
        {
            using (_host)
            {
                await _host.StopAsync(TimeSpan.FromSeconds(5));
            }
        }
    }
}
