namespace Npm.Renovator.ConsoleApp.Models;

internal sealed record ConsoleJourneyState
{
    public Func<CancellationToken, Task<ConsoleJourneyState>>? NextMove { get; set; }
}