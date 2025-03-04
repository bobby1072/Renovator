using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npm.Renovator.ConsoleApp.Abstract;

namespace Npm.Renovator.ConsoleApp;

public static class Program
{
    public static async Task Main()
    {
        var configurationManager = new ConfigurationManager();
        
        configurationManager.AddJsonFile(Path.GetFullPath("appsettings.json"));
        
        var serviceCollection = new ServiceCollection();
     
        


        serviceCollection.AddSingleton<IConfigurationManager>(configurationManager);

        serviceCollection.AddTransient<IConsoleApp, Concrete.ConsoleApp>();
        
        
        await using var scope = serviceCollection.BuildServiceProvider();
        
        await using var asyncScope = scope.CreateAsyncScope();
        
        
        await asyncScope.ServiceProvider.GetRequiredService<IConsoleApp>().ExecuteAsync();
    }
}



