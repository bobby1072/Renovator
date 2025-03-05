using Microsoft.Extensions.DependencyInjection;
using Npm.Renovator.Repo.Services.Abstract;
using Npm.Renovator.Repo.Services.Concrete;

namespace Npm.Renovator.Repo.Services.Extensions;

public static class RepoReaderServiceCollectionExtensions
{
    public static IServiceCollection AddRepoReader(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IRepoReaderService, RepoReaderService>();
        
        return serviceCollection;
    }
}