namespace Npm.Renovator.Domain.Models.Views;

public sealed record CurrentPackageVersionsAndPotentialUpgradesView
{
    public required IReadOnlyCollection<CurrentPackageVersionsAndPotentialUpgradesViewSinglePackage> AllPackages { get; init; }
}