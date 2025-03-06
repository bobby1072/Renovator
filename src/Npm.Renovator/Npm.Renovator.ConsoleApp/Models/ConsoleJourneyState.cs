namespace Npm.Renovator.ConsoleApp.Models;

internal record ConsoleJourneyState
{
    public Func<CancellationToken,Task<ConsoleJourneyState>>? NextMove { get; set; }
}