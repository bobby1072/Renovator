namespace Npm.Renovator.Domain.Models.Views;

public record CurrentPackageVersionsAndPotentialUpgradesViewPotentialNewVersion
{
    public required string CurrentVersion { get; init; }
    public required DateTime ReleaseDate { get; init; }
}