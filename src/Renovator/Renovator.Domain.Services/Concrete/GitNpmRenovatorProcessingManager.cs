﻿using BT.Common.FastArray.Proto;
using Microsoft.Extensions.Logging;
using Renovator.Common.Exceptions;
using Renovator.Common.Helpers;
using Renovator.Domain.Models;
using Renovator.Domain.Models.Extensions;
using Renovator.Domain.Models.Views;
using Renovator.Domain.Services.Abstract;
using Renovator.NpmHttpClient.Abstract;

namespace Renovator.Domain.Services.Concrete
{
    internal sealed class GitNpmRenovatorProcessingManager : NpmRenovatorProcessingManager, IGitNpmRenovatorProcessingManager
    {
        private readonly ILogger<GitNpmRenovatorProcessingManager> _logger;
        private readonly IGitCommandService _gitCommandService;
        private TempRepositoryFromGit? _tempRepoInstance;
        public GitNpmRenovatorProcessingManager(IGitCommandService gitCommandService,
            ILogger<GitNpmRenovatorProcessingManager> logger,
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
        public void Dispose()
        {
            _tempRepoInstance?.Dispose();
        }
        public async Task<RenovatorOutcome<IReadOnlyCollection<CurrentPackageVersionsAndPotentialUpgradesViewWithFullPath>>> GetTempRepoWithCurrentPackageVersionAndPotentialUpgradesViewAsync(
            Uri gitRepoUri,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var tempRepo = await GetTempRepo(gitRepoUri, cancellationToken);

                var allPackageJsons = await _reader.AnalyseMultiplePackageJsonDependenciesAsync(tempRepo.FullPathTo, cancellationToken);

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
                var renException = RenovatorExceptionHelper.CreateRenovatorException(
                        nameof(GetTempRepoWithCurrentPackageVersionAndPotentialUpgradesViewAsync),
                        ex);

                LogRenovatorException(renException);


                return new RenovatorOutcome<IReadOnlyCollection<CurrentPackageVersionsAndPotentialUpgradesViewWithFullPath>>
                {
                    RenovatorException = renException
                };
            }
        }

        public async Task<RenovatorOutcome<IReadOnlyCollection<LazyPackageJson>>> FindAllPackageJsonsInTempRepoAsync(
            Uri gitRepoUri,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var tempRepo = await GetTempRepo(gitRepoUri, cancellationToken);

                var allPackageJsons = await _reader.AnalyseMultiplePackageJsonDependenciesAsync(tempRepo.FullPathTo, cancellationToken);

                return new RenovatorOutcome<IReadOnlyCollection<LazyPackageJson>>
                {
                    Data = allPackageJsons
                };
            }
            catch (Exception ex)
            {
                var renException = RenovatorExceptionHelper.CreateRenovatorException(
                        nameof(FindAllPackageJsonsInTempRepoAsync),
                        ex);

                LogRenovatorException(renException);

                return new RenovatorOutcome<IReadOnlyCollection<LazyPackageJson>>
                {
                    RenovatorException = renException
                };
            }
        }
        public async Task<RenovatorOutcome<ProcessCommandResult>> AttemptToRenovateTempRepoAsync(GitDependencyUpgradeBuilder upgradeBuilder, CancellationToken token = default)
        {
            try
            {
                var tempRepo = await GetTempRepo(upgradeBuilder.RemoteRepoLocation, token);

                var allPackageJsons = await _reader.AnalyseMultiplePackageJsonDependenciesAsync(tempRepo.FullPathTo, token);

                var analysedDependencies
                    = allPackageJsons.FindPackageJsonByPropertyWithin(("name", upgradeBuilder.NameInPackageJson))
                        ?? throw new InvalidOperationException("Could not find package json in the temp repo with that applictaion name");

                var localUpgradeBuilder = LocalDependencyUpgradeBuilder.Create(analysedDependencies.FullLocalPathToPackageJson);
                
                foreach (var upgs in upgradeBuilder.ReadonlyUpgradesView)
                {
                    localUpgradeBuilder.AddUpgrade(upgs.Key, upgs.Value);
                }
                
                return await AttemptToRenovateLocalSystemRepoAsync(localUpgradeBuilder, token);
            }
            catch (Exception ex)
            {
                var renovatorException = RenovatorExceptionHelper.CreateRenovatorException(nameof(AttemptToRenovateTempRepoAsync), ex);

                LogRenovatorException(renovatorException);

                return new RenovatorOutcome<ProcessCommandResult>
                {
                    RenovatorException = renovatorException
                };
            }
        }
        protected override void LogRenovatorException(RenovatorException ex)
        {
            _logger.LogError(ex, "GitNpmRenovatorProcessingManager caught an exception with the inner message: {InnerMessage}",
                ex.InnerException?.Message);
        } 
        private async Task<TempRepositoryFromGit> GetTempRepo(
            Uri gitRepoUri,
            CancellationToken cancellationToken
        )
        {
            if (gitRepoUri.Equals(_tempRepoInstance?.GitRepoLocation))
            {
                return _tempRepoInstance;
            }
            _tempRepoInstance?.Dispose();
            _tempRepoInstance = null;

            var tempRepo = await _gitCommandService.CheckoutRemoteRepoToLocalTempStoreAsync(gitRepoUri, cancellationToken);

            if (tempRepo.Data is null)
            {
                _logger.LogError("Git clone command failed with error output: {Output} and standard output {StdOutput}",
                    tempRepo.ExceptionOutput,
                    tempRepo.Output
                );
                throw new InvalidOperationException("Failed to clone repo");
            }

            _tempRepoInstance = tempRepo.Data;

            return _tempRepoInstance;
        }
    }
}
