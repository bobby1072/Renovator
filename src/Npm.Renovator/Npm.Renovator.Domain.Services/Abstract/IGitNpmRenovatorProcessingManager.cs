using Npm.Renovator.Domain.Models;
using Npm.Renovator.Domain.Models.Views;

namespace Npm.Renovator.Domain.Services.Abstract
{
    public interface IGitNpmRenovatorProcessingManager : IDisposable
    {
        Task<RenovatorOutcome<ProcessCommandResult>> AttemptToRenovateTempRepo(
            GitDependencyUpgradeBuilder upgradeBuilder,
            CancellationToken token = default
        );
        Task<
            RenovatorOutcome<
                IReadOnlyCollection<CurrentPackageVersionsAndPotentialUpgradesViewWithFullPath>
            >
        > GetTempRepoWithCurrentPackageVersionAndPotentialUpgradesView(
            Uri gitRepoUri,
            CancellationToken cancellationToken = default
        );
        Task<RenovatorOutcome<IReadOnlyCollection<LazyPackageJson>>> FindAllPackageJsonsInTempRepo(
            Uri gitRepoUri,
            CancellationToken cancellationToken = default
        );
    }
}
