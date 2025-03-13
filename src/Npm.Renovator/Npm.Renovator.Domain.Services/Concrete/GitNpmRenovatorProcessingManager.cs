using Microsoft.Extensions.Logging;
using Npm.Renovator.Domain.Models;
using Npm.Renovator.Domain.Models.Views;
using Npm.Renovator.Domain.Services.Abstract;

namespace Npm.Renovator.Domain.Services.Concrete
{
    internal class GitNpmRenovatorProcessingManager
    {
        private readonly ILogger<GitNpmRenovatorProcessingManager> _logger;
        private readonly IGitCommandService _gitCommandService;
        private readonly INpmRenovatorProcessingManager _npmRenovatorProcessingManager;
        private readonly IRepoExplorerService _repoExplorerService;
        public GitNpmRenovatorProcessingManager(IGitCommandService gitCommandService, INpmRenovatorProcessingManager npmRenovatorProcessingManager, IRepoExplorerService repoExplorerService, ILogger<GitNpmRenovatorProcessingManager> logger)
        {
            _gitCommandService = gitCommandService;
            _npmRenovatorProcessingManager = npmRenovatorProcessingManager;
            _logger = logger;
            _repoExplorerService = repoExplorerService;
        }
    
        public async Task<RenovatorOutcome<IReadOnlyCollection<CurrentPackageVersionsAndPotentialUpgradesViewWithFullPath>>> GetTempRepoWithCurrentPackageVersionAndPotentialUpgradesView(
            Uri gitRepoUri,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                using var tempRepo = await _gitCommandService.CheckoutRemoteRepoToLocalTempStoreAsync(gitRepoUri, cancellationToken);

                if(!tempRepo.IsSuccess || tempRepo.Data is null)
                {
                    _logger.LogError("Git clone command failed with error output: {Output} and standard output {StdOutput}",
                        tempRepo.ExceptionOutput,
                        tempRepo.Output
                    );
                    throw new InvalidOperationException("Failed to clone repo");
                }

                var allPackageJsons = await _repoExplorerService.AnalyseMultiplePackageJsonDependenciesAsync(tempRepo.Data.FullPathTo, cancellationToken);


                
            }
            catch
            {

            }

            throw new NotImplementedException();
        }

        private async Task<>
    }
}
