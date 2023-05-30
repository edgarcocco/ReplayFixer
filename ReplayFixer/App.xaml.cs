using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReplayFixer.Services;
using ReplayFixer.Services.Contracts;
using ReplayFixer.Views;
using System;
using System.Windows;
using AutoUpdaterDotNET;
using Microsoft.Extensions.Options;
using Wpf.Ui.Demo.Services;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;
using Serilog;
using Serilog.Hosting;
using System.Globalization;
using Microsoft.Extensions.Localization;

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
                        services.Configure<AppConfig>(context.Configuration.GetSection("AppConfig"));
                        services.AddHostedService<ApplicationHostService>();
                        services.AddOptions();
                        services.AddLogging();
                        services.AddLocalization(options =>
                        {
                            options.ResourcesPath = "Resources";
                        });
                        services.AddTransient<MessageService>();


                        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = new CultureInfo(context.Configuration.GetSection("AppConfig")["Language"] ?? "en-US");
                        services.AddSingleton<IPageService, PageService>();
                        services.AddSingleton<IDialogService, DialogService>();
                        services.AddSingleton<IThemeService, ThemeService>();
                        services.AddSingleton<ISnackbarService, SnackbarService>();
                        services.AddSingleton<INavigationService, NavigationService>();

                        //services.AddSingleton<INotifyIconService, NotifyIconService>();

                        services.AddScoped(typeof(IDelimitedFileService<>), typeof(DelimitedFileService<>));
                        services.AddScoped(typeof(IFileService<>), typeof(FileService<>));


                        services.AddScoped<INavigationWindow, Container>();

                        services.AddScoped<Views.Pages.Dashboard>();
                        services.AddScoped<ViewModels.DashboardViewModel>();

                        services.AddScoped<Views.Pages.MethodOnePage>();
                        services.AddScoped<ViewModels.MethodOneViewModel>();

                        services.AddScoped<Views.Pages.SettingsPage>();
                    })
                    .ConfigureLogging((context, logging) =>
                    {
                        /*logging.AddConfiguration(context.Configuration.GetSection("Logging"));
                        logging.AddConsole();
                        //logging.AddFile($"{AppDomain.CurrentDomain.BaseDirectory}/event.log", LogLevel.Information, new );
                        logging.AddFile($"{AppDomain.CurrentDomain.BaseDirectory}/debug.log");
#if DEBUG
                        logging.AddDebug();
#endif
                        */
                    }).UseSerilog((context, services, loggerConfiguration) => {
                        loggerConfiguration
                        .ReadFrom.Configuration(context.Configuration)
                        .WriteTo.Console()
                        .WriteTo.File($"{AppDomain.CurrentDomain.BaseDirectory}/debug-{DateTime.Now.ToString("yyyyMMd")}.log", Serilog.Events.LogEventLevel.Debug);
                    })
                    .Build();

        }

        private async void Application_Startup(object sender, StartupEventArgs e)
        {

            await _host.StartAsync();
                        var container = _host.Services.GetService<Container>();
            container?.Show();

            var appConfig = _host.Services.GetService<IOptions<AppConfig>>()?.Value;
            if (appConfig != null) AutoUpdater.Start(appConfig.AutoUpdaterFile);

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
