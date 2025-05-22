using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PrinterApp.ApplicationValidator;
using PrinterApp.Configuration;
using PrinterApp.Core;

namespace PrinterApp
{
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
                    services.AddOptions<ApplicationConfiguration>()
                            .Bind(context.Configuration.GetSection("AppConfiguration"))
                            .ValidateDataAnnotations()
                            .ValidateOnStart();

                    services.AddSingleton<ApplicationBootstrapper>();
                    services.AddSingleton<IValidateOptions<ApplicationConfiguration>, ApplicationConfigurationValidator>();
                })
                .Build();

            var app = host.Services.GetRequiredService<ApplicationBootstrapper>();
            await app.RunAsync();
        }
    }
}