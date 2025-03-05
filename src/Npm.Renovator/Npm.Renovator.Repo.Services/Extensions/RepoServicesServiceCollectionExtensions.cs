using Microsoft.Extensions.DependencyInjection;
using Npm.Renovator.Repo.Services.Abstract;
using Npm.Renovator.Repo.Services.Concrete;

namespace Npm.Renovator.Repo.Services.Extensions;

public static class RepoServicesServiceCollectionExtensions
{
    public static IServiceCollection AddRepoServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IRepoReaderService, RepoReaderService>();
        
        return serviceCollection;
    }
}