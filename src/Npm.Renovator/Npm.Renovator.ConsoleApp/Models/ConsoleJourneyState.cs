namespace Npm.Renovator.ConsoleApp.Models;

internal record ConsoleJourneyState
{
    public required Func<CancellationToken,Task<ConsoleJourneyState>> NextMove { get; set; }
}