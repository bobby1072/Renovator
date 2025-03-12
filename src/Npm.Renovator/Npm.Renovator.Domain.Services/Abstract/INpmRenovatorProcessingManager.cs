using Npm.Renovator.Domain.Models;
using Npm.Renovator.Domain.Models.Views;

namespace Npm.Renovator.Domain.Services.Abstract;

public interface INpmRenovatorProcessingManager
{
    Task<RenovatorOutcome<ProcessCommandResult>> AttemptToRenovateLocalSystemRepoAsync(DependencyUpgradeBuilder upgradeBuilder, CancellationToken cancellationToken = default);
    Task<RenovatorOutcome<CurrentPackageVersionsAndPotentialUpgradesView>> GetCurrentPackageVersionAndPotentialUpgradesViewForLocalSystemRepoAsync(
        string localSystemFilePathToPackageJson, CancellationToken cancellationToken = default);
}