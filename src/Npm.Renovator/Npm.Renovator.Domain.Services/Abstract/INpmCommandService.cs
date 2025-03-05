using Npm.Renovator.Domain.Models;

namespace Npm.Renovator.Domain.Services.Abstract;

internal interface INpmCommandService
{
    Task<NpmCommandResults> RunNpmInstallAsync(string localSystemFilePathToPackageJson, CancellationToken cancellationToken = default);
}