using Renovator.Domain.Models;

namespace Renovator.Domain.Services.Abstract;

internal interface INpmCommandService
{
    Task<ProcessCommandResult> RunNpmInstallAsync(string workingDirectory, CancellationToken cancellationToken = default);
}