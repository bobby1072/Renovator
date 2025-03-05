using Npm.Renovator.Domain.Models.Views;

namespace Npm.Renovator.Domain.Services.Abstract;

public interface INpmRenovatorProcessingManager
{
    Task<CurrentPackageVersionsAndPotentialUpgradesView> GetCurrentPackageVersionAndPotentialUpgradesViewAsync(
        string filePath, CancellationToken cancellationToken = default);
}