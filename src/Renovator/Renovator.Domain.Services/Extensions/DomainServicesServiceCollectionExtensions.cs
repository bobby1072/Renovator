using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Renovator.Common.Helpers;
using Renovator.Domain.Services.Abstract;
using Renovator.Domain.Services.Concrete;
using Renovator.NpmHttpClient.Extensions;

namespace Renovator.Domain.Services.Extensions;

public static class DomainServicesServiceCollectionExtensions
{
    public static IServiceCollection AddRenovatorApplication(this IServiceCollection serviceCollection, IConfiguration configurationManager)
    {
        serviceCollection
            .AddLogging()
            .AddHttpClient()
            .AddNpmHttpClient(configurationManager)
            .AddScoped<INpmRenovatorProcessingManager, NpmRenovatorProcessingManager>()
            .AddScoped<IGitNpmRenovatorProcessingManager, GitNpmRenovatorProcessingManager>()
            .AddScoped<IRepoExplorerService, RepoExplorerService>()
            .AddScoped<IProcessExecutor, ProcessExecutor>()
            .AddTransient<ComputerResourceCheckProcessCommand>()
            .AddTransient<NpmInstallProcessCommand>()
            .AddTransient<CheckoutRemoteRepoToLocalTempStoreProcessCommand>()
            .AddTransient(_ =>
            {
                var process = new Process();
                process.StartInfo = ProcessHelper.GetDefaultProcessStartInfo();

                return process;
            });
        
        return serviceCollection;
    }
}