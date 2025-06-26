using Npm.Renovator.Domain.Models;

namespace Npm.Renovator.Domain.Services.Abstract;

internal interface INpmCommandService
{
    Task<ProcessCommandResult> RunNpmInstallAsync(string workingDirectory, CancellationToken cancellationToken = default);
}