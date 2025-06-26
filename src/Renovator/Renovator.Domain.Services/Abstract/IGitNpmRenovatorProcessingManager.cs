using Renovator.Domain.Models;
using Renovator.Domain.Models.Views;

namespace Renovator.Domain.Services.Abstract
{
    public interface IGitNpmRenovatorProcessingManager : IDisposable
    {
        Task<RenovatorOutcome<ProcessCommandResult>> AttemptToRenovateTempRepoAsync(
            GitDependencyUpgradeBuilder upgradeBuilder,
            CancellationToken token = default
        );
        Task<
            RenovatorOutcome<
                IReadOnlyCollection<CurrentPackageVersionsAndPotentialUpgradesViewWithFullPath>
            >
        > GetTempRepoWithCurrentPackageVersionAndPotentialUpgradesViewAsync(
            Uri gitRepoUri,
            CancellationToken cancellationToken = default
        );
        Task<RenovatorOutcome<IReadOnlyCollection<LazyPackageJson>>> FindAllPackageJsonsInTempRepoAsync(
            Uri gitRepoUri,
            CancellationToken cancellationToken = default
        );
    }
}
