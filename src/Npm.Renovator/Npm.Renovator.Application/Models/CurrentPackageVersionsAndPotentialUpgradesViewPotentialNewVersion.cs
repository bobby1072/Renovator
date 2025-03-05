namespace Npm.Renovator.Application.Models;

public record CurrentPackageVersionsAndPotentialUpgradesViewPotentialNewVersion
{
    public required string CurrentVersion { get; init; }
    public required DateTime ReleaseDate { get; init; }
}