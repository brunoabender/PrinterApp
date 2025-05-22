using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PrinterApp.Configuration;
using PrinterApp.Core;

public class Program
{
    private static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: true)
                    .AddEnvironmentVariables()
                    .AddCommandLine(args);
        })
    .ConfigureServices((context, services) =>
    {
        services.Configure<AppConfiguration>(context.Configuration.GetSection("AppConfiguration"));
        services.AddSingleton<AppBootstrapper>();
    })
    .Build();

        var app = host.Services.GetRequiredService<AppBootstrapper>();
        await app.RunAsync();
    }
}