namespace Npm.Renovator.Application.Models;

public sealed record CurrentPackageVersionsAndPotentialUpgradesView
{ 
    public required IReadOnlyCollection<CurrentPackageVersionsAndPotentialUpgradesViewSinglePackage> AllPackages { get; init; }   
}