using System.Diagnostics;
using Renovator.Domain.Models;

namespace Renovator.Domain.Services.Abstract;

public abstract class BaseProcessCommand
{
    public abstract string Name { get; }
    protected readonly Process _processRunner;
    public BaseProcessCommand(Process processRunner)
    {
        _processRunner = processRunner;
    }
}

public abstract class BaseProcessCommand<TProcessCommandResult>: BaseProcessCommand where TProcessCommandResult : ProcessCommandResult
{
    public BaseProcessCommand(Process processRunner): base(processRunner) {}
    public abstract Task<TProcessCommandResult> ExecuteCommandAsync(CancellationToken cancellationToken = default);
}

public abstract class BaseProcessCommand<TProcessCommandInput, TProcessCommandResult> : BaseProcessCommand where TProcessCommandResult : ProcessCommandResult
{
    public BaseProcessCommand(Process processRunner): base(processRunner) {}
    public abstract Task<TProcessCommandResult> ExecuteCommandAsync(TProcessCommandInput input, CancellationToken token = default);
}