namespace Npm.Renovator.Application.Models;

public record CurrentPackageVersionsAndPotentialUpgradesView
{ 
    public required IReadOnlyCollection<CurrentPackageVersionsAndPotentialUpgradesViewSinglePackage> AllPackages { get; init; }   
}