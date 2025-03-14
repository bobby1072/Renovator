using BT.Common.FastArray.Proto;
using Microsoft.Extensions.Logging;
using Npm.Renovator.Common.Helpers;
using Npm.Renovator.Domain.Models;
using Npm.Renovator.Domain.Models.Views;
using Npm.Renovator.Domain.Services.Abstract;
using Npm.Renovator.NpmHttpClient.Abstract;

namespace Npm.Renovator.Domain.Services.Concrete
{
    internal class GitNpmRenovatorProcessingManager : NpmRenovatorProcessingManager, IGitNpmRenovatorProcessingManager
    {
        private readonly ILogger<GitNpmRenovatorProcessingManager> _logger;
        private readonly IGitCommandService _gitCommandService;
        public GitNpmRenovatorProcessingManager(IGitCommandService gitCommandService, ILogger<GitNpmRenovatorProcessingManager> logger,
            INpmJsRegistryHttpClient npmJsRegistryHttpClient,
            IRepoExplorerService reader,
            ILogger<NpmRenovatorProcessingManager> baseLogger,
            INpmCommandService npmCommandService) :
                base(npmJsRegistryHttpClient,
                    reader,
                    baseLogger,
                    npmCommandService)
        {
            _gitCommandService = gitCommandService;
            _logger = logger;
        }

        public async Task<RenovatorOutcome<IReadOnlyCollection<CurrentPackageVersionsAndPotentialUpgradesViewWithFullPath>>> GetTempRepoWithCurrentPackageVersionAndPotentialUpgradesView(
            Uri gitRepoUri,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                using var tempRepo = await _gitCommandService.CheckoutRemoteRepoToLocalTempStoreAsync(gitRepoUri, cancellationToken);

                if (!tempRepo.IsSuccess || tempRepo.Data is null)
                {
                    _logger.LogError("Git clone command failed with error output: {Output} and standard output {StdOutput}",
                        tempRepo.ExceptionOutput,
                        tempRepo.Output
                    );
                    throw new InvalidOperationException("Failed to clone repo");
                }

                var allPackageJsons = await _reader.AnalyseMultiplePackageJsonDependenciesAsync(tempRepo.Data.FullPathTo, cancellationToken);

                var uniquePackageDict =
                    allPackageJsons
                    .FastArraySelect(x => x.OriginalPackageJsonDependencies.Dependencies.Union(x.OriginalPackageJsonDependencies.DevDependencies))
                    .SelectMany(x => x)
                    .DistinctBy(x => x.Key)
                    .ToDictionary();

                var rawResult = await GetPotentialNewPackagesFromRegistry(uniquePackageDict);

                var resultToReturn = allPackageJsons.FastArraySelect(x =>
                {
                    return new CurrentPackageVersionsAndPotentialUpgradesViewWithFullPath
                    {
                        AllPackages = new List<CurrentPackageVersionsAndPotentialUpgradesViewSinglePackage>()
                                        .Union(GetListOfPotentialNewPackages(x.OriginalPackageJsonDependencies.Dependencies, rawResult))
                                        .Union(GetListOfPotentialNewPackages(x.OriginalPackageJsonDependencies.DevDependencies, rawResult)).ToArray(),
                        FullPathToPackageJson = x.FullLocalPathToPackageJson
                    };
                }).ToArray();

                return new RenovatorOutcome<IReadOnlyCollection<CurrentPackageVersionsAndPotentialUpgradesViewWithFullPath>>
                {
                    Data = resultToReturn
                };
            }
            catch (Exception ex)
            {
                return new RenovatorOutcome<IReadOnlyCollection<CurrentPackageVersionsAndPotentialUpgradesViewWithFullPath>>
                {
                    RenovatorException = RenovatorExceptionHelper.CreateRenovatorException(
                        nameof(GetTempRepoWithCurrentPackageVersionAndPotentialUpgradesView),
                        ex)
                };
            }
        }

    }
}
