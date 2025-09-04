using Renovator.Domain.Models;

namespace Renovator.Domain.Services.Abstract;

internal interface IProcessExecutor
{
    Task<TProcessCommandResult> RunCommandAsync<TProcessCommand, TProcessCommandResult>(CancellationToken ct = default)
        where TProcessCommand : BaseProcessCommand<TProcessCommandResult>
        where TProcessCommandResult : ProcessCommandResult;
    Task<TProcessCommandResult> RunCommandAsync<TProcessCommand, TProcessCommandInput, TProcessCommandResult>(TProcessCommandInput input, CancellationToken ct = default)
        where TProcessCommand : BaseProcessCommand<TProcessCommandInput, TProcessCommandResult>
        where TProcessCommandResult: ProcessCommandResult;
}