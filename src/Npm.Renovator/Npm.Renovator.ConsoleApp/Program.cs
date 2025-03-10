using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npm.Renovator.ConsoleApp.Abstract;
using Npm.Renovator.Domain.Services.Extensions;

namespace Npm.Renovator.ConsoleApp;

public static class Program
{
    public static async Task Main()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.AddJsonFile(Path.GetFullPath("appsettings.json"));
            })
            .ConfigureServices((context, services) =>
            {
                services
                    .AddRenovatorApplication(context.Configuration)
                    .AddLogging(opts =>
                    {
                        opts.SetMinimumLevel(LogLevel.None);
                    })
                    .AddTransient<IConsoleApplicationService, Concrete.ConsoleApplicationService>();
            })
            .Build();

        await host.StartAsync();

        await (host.Services.GetRequiredService<IConsoleApplicationService>()).ExecuteAsync();

        await host.StopAsync();
    }
}



