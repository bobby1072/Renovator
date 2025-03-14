using Npm.Renovator.Domain.Models.Views;
using Npm.Renovator.Domain.Models;

namespace Npm.Renovator.Domain.Services.Abstract
{
    public interface IGitNpmRenovatorProcessingManager
    {
        Task<RenovatorOutcome<IReadOnlyCollection<CurrentPackageVersionsAndPotentialUpgradesViewWithFullPath>>> GetTempRepoWithCurrentPackageVersionAndPotentialUpgradesView(
            Uri gitRepoUri,
            CancellationToken cancellationToken = default
        );
    }
}
