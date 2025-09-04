using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReplayFixer.Tool;
using Serilog;

using IHost host = CreateHostBuilder(args).Build();
using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
    services.GetRequiredService<App>().Run(args);
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}
static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    Serilog.ILogger serilogLogger = new LoggerConfiguration()
                .WriteTo.Console(
                    outputTemplate: "{Timestamp:HH:mm:ss} {Level:u4}: {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();
                    services.AddLogging((builder) =>
                    {
                        builder.ClearProviders();
                        builder.AddSerilog(serilogLogger);
                    });
                    //services.AddSingleton<IMessages, Messages>();
                    services.AddSingleton<App>();
                });
}