using Npm.Renovator.Application.Models;

namespace Npm.Renovator.Application.Services.Abstract;

public interface INpmRenovatorProcessingManager
{
    Task<CurrentPackageVersionsAndPotentialUpgradesView> GetCurrentPackageVersionAndPotentialUpgradesViewAsync(
        string filePath, CancellationToken cancellationToken = default);
}