using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Renovator.ConsoleApp.Abstract;
using Renovator.ConsoleApp.Concrete;
using Renovator.Domain.Services.Extensions;

try
{
    using var host = Host.CreateDefaultBuilder()
        .ConfigureAppConfiguration(config =>
        {
            config
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile(Path.GetFullPath("appsettings.json"), false);
        })
        .ConfigureServices((context, services) =>
        {
            services
                .AddRenovatorApplication(context.Configuration)
                .AddTransient<IConsoleApplicationService, ConsoleApplicationService>()
                .AddHostedService<ConsoleRunnerService>();
        })
        .ConfigureLogging(opts =>
        {
            opts.SetMinimumLevel(LogLevel.None);
        })
        .Build();


    await host.RunAsync();
}
catch(Exception e)
{
    Console.Clear();
    Console.WriteLine($"Exception occured during setup with message: {Environment.NewLine}");
    Console.WriteLine(e.Message);
}


