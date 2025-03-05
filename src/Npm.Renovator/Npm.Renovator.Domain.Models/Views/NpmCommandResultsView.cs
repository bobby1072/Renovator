namespace Npm.Renovator.Domain.Models.Views;

public sealed record NpmCommandResultsView
{
    public string? Output { get; init; }
    public string? Error { get; init; }
}