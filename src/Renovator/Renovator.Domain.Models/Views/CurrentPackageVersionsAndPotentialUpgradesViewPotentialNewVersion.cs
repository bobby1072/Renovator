namespace Renovator.Domain.Models.Views;

public sealed record CurrentPackageVersionsAndPotentialUpgradesViewPotentialNewVersion
{
    public required string CurrentVersion { get; init; }
    public required DateTime ReleaseDate { get; init; }
}