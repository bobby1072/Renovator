using Microsoft.Extensions.DependencyInjection;
using Npm.Renovator.RepoReader.Abstract;
using Npm.Renovator.RepoReader.Concrete;

namespace Npm.Renovator.RepoReader.Extensions;

public static class RepoReaderServiceCollectionExtensions
{
    public static IServiceCollection AddRepoReader(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IRepoReaderService, RepoReaderService>();
        
        return serviceCollection;
    }
}