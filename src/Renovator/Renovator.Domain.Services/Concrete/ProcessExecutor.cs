using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Renovator.Domain.Models;
using Renovator.Domain.Services.Abstract;

namespace Renovator.Domain.Services.Concrete;

internal sealed class ProcessExecutor : IProcessExecutor
{
    private readonly ILogger<ProcessExecutor> _logger;
    private readonly IServiceProvider _serviceProvider;
    public ProcessExecutor(ILogger<ProcessExecutor> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    public async Task<TProcessCommandResult> RunCommandAsync<TProcessCommand, TProcessCommandResult>(CancellationToken ct = default)
        where TProcessCommand : BaseProcessCommand<TProcessCommandResult>
        where TProcessCommandResult : ProcessCommandResult
    {
        var foundProcessCommand = _serviceProvider.GetRequiredService<TProcessCommand>();

        _logger.LogInformation("Attempting to execute {ProcessName} process...", foundProcessCommand.Name);

        return await foundProcessCommand.ExecuteCommandAsync(ct);
    }
    public async Task<TProcessCommandResult> RunCommandAsync<TProcessCommand, TProcessCommandInput, TProcessCommandResult>(TProcessCommandInput input, CancellationToken ct = default)
        where TProcessCommand : BaseProcessCommand<TProcessCommandInput, TProcessCommandResult>
        where TProcessCommandResult : ProcessCommandResult
    {
        var foundProcessCommand = _serviceProvider.GetRequiredService<TProcessCommand>();

        _logger.LogInformation("Attempting to execute {ProcessName} process...", foundProcessCommand.Name);

        return await foundProcessCommand.ExecuteCommandAsync(input, ct);
    }
}