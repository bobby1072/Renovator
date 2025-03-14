namespace Npm.Renovator.Domain.Models.Views
{
    public record CurrentPackageVersionsAndPotentialUpgradesViewWithFullPath: CurrentPackageVersionsAndPotentialUpgradesView
    {
        public required string FullPathToPackageJson { get; init; }
    }
}
