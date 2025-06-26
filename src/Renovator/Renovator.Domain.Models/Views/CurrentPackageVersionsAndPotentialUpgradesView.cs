namespace Renovator.Domain.Models.Views;

public record CurrentPackageVersionsAndPotentialUpgradesView
{
    public required IReadOnlyCollection<CurrentPackageVersionsAndPotentialUpgradesViewSinglePackage> AllPackages { get; init; }
}