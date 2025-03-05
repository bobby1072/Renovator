using Microsoft.Extensions.DependencyInjection;
using Npm.Renovator.Domain.Services.Abstract;
using Npm.Renovator.Domain.Services.Concrete;

namespace Npm.Renovator.Domain.Services.Extensions;

public static class RepoServicesServiceCollectionExtensions
{
    public static IServiceCollection AddRepoServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IRepoReaderService, RepoReaderService>();
        
        return serviceCollection;
    }
}