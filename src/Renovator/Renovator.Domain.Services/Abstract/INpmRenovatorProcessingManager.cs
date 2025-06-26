using Renovator.Domain.Models;
using Renovator.Domain.Models.Views;

namespace Renovator.Domain.Services.Abstract;

public interface INpmRenovatorProcessingManager
{
    Task<RenovatorOutcome<ProcessCommandResult>> AttemptToRenovateLocalSystemRepoAsync(LocalDependencyUpgradeBuilder upgradeBuilder, CancellationToken cancellationToken = default);
    Task<RenovatorOutcome<CurrentPackageVersionsAndPotentialUpgradesView>> GetCurrentPackageVersionAndPotentialUpgradesViewForLocalSystemRepoAsync(
        string localSystemFilePathToPackageJson, CancellationToken cancellationToken = default);
}