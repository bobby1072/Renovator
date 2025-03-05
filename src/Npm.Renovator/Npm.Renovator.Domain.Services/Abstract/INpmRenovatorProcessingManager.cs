using Npm.Renovator.Domain.Models;
using Npm.Renovator.Domain.Models.Views;

namespace Npm.Renovator.Domain.Services.Abstract;

public interface INpmRenovatorProcessingManager
{
    Task<RenovatorOutcome<NpmCommandResults>> AttemptToRenovateRepoAsync(DependencyUpgradeBuilder upgradeBuilder, CancellationToken cancellationToken = default);
    Task<RenovatorOutcome<CurrentPackageVersionsAndPotentialUpgradesView>> GetCurrentPackageVersionAndPotentialUpgradesViewAsync(
        string filePath, CancellationToken cancellationToken = default);
}