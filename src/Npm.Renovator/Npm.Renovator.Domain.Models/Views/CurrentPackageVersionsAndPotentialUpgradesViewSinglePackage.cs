﻿namespace Npm.Renovator.Domain.Models.Views;

public record CurrentPackageVersionsAndPotentialUpgradesViewSinglePackage
{
    public required string NameOnNpm { get; init; }

    public required string CurrentVersion { get; init; }

    public required IReadOnlyCollection<CurrentPackageVersionsAndPotentialUpgradesViewPotentialNewVersion> PotentialNewVersions { get; init; }
}