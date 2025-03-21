using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npm.Renovator.ConsoleApp.Abstract;
using Npm.Renovator.ConsoleApp.Concrete;
using Npm.Renovator.Domain.Services.Extensions;

namespace Npm.Renovator.ConsoleApp;

public static class Program
{
    public static async Task Main()
    {
        try
        {
            using var host = Host.CreateDefaultBuilder()
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
                        .AddTransient<IConsoleApplicationService, Concrete.ConsoleApplicationService>()
                        .AddHostedService<ConsoleRunnerService>();
                })
                .Build();


            await host.RunAsync();
        }
        catch(System.Exception e)
        {
            Console.Clear();
            Console.WriteLine($"Exception occured during setup with message: {Environment.NewLine}");
            Console.WriteLine(e.Message);
        }
    }
}



