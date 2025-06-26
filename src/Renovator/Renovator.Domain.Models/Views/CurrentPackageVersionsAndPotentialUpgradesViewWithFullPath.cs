namespace Renovator.Domain.Models.Views
{
    public sealed record CurrentPackageVersionsAndPotentialUpgradesViewWithFullPath: CurrentPackageVersionsAndPotentialUpgradesView
    {
        public required string FullPathToPackageJson { get; init; }
    }
}
