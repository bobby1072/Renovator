using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npm.Renovator.ConsoleApp.Abstract;
using Npm.Renovator.Domain.Services.Extensions;

namespace Npm.Renovator.ConsoleApp;

public static class Program
{
    public static async Task Main()
    {
        var configurationManager = new ConfigurationManager();
        
        configurationManager.AddJsonFile(Path.GetFullPath("appsettings.json"));
        
        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddRenovatorApplication(configurationManager)
            .AddLogging(opts =>
            {
                opts.SetMinimumLevel(LogLevel.None);
            })
            .AddSingleton<IConfigurationManager>(configurationManager)
            .AddTransient<IConsoleApplicationService, Concrete.ConsoleApplicationService>();

        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        
        await using var asyncScope = serviceProvider.CreateAsyncScope();
        
        
        await asyncScope.ServiceProvider.GetRequiredService<IConsoleApplicationService>().ExecuteAsync();
    }
}



