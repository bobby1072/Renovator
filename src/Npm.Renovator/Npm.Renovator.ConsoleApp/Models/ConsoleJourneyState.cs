namespace Npm.Renovator.ConsoleApp.Models;

public class ConsoleJourneyState
{
    public required Func<CancellationToken,Task<ConsoleJourneyState>> NextMove { get; set; }
}