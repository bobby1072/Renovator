using Npm.Renovator.Domain.Models.Views;

namespace Npm.Renovator.Domain.Services.Abstract;

internal interface INpmCommandService
{
    Task<NpmCommandResults> RunNpmInstallAsync(string filePath, CancellationToken cancellationToken = default);
}