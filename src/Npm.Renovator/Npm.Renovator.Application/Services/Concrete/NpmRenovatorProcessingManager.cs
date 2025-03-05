using Microsoft.Extensions.Logging;
using Npm.Renovator.Application.Models;
using Npm.Renovator.Application.Services.Abstract;
using Npm.Renovator.NpmHttpClient.Abstract;
using Npm.Renovator.RepoReader.Abstract;

namespace Npm.Renovator.Application.Services.Concrete;

internal class NpmRenovatorProcessingManager: INpmRenovatorProcessingManager
{
    private readonly INpmJsRegistryHttpClient _npmJsRegistryHttpClient;
    private readonly IRepoReaderService _reader;
    private readonly ILogger<NpmRenovatorProcessingManager> _logger;
    
    public NpmRenovatorProcessingManager(INpmJsRegistryHttpClient npmJsRegistryHttpClient, IRepoReaderService reader,
        ILogger<NpmRenovatorProcessingManager> logger)
    {
        _npmJsRegistryHttpClient = npmJsRegistryHttpClient;
        _reader = reader;
        _logger = logger;
    }

    public Task<CurrentPackageVersionsAndPotentialUpgradesView> GetCurrentPackageVersionAndPotentialUpgradesView(string filePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}