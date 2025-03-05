namespace Npm.Renovator.Domain.Models.Views;

public record NpmCommandResults
{
    public string? Output { get; init; }
    public string? Exception { get; init; }
}