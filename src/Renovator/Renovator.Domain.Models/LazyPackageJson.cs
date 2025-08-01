﻿using System.Text.Json.Nodes;

namespace Renovator.Domain.Models
{
    public sealed record LazyPackageJson
    {
        public required string FullLocalPathToPackageJson { get; init; }
        public required Lazy<JsonObject> FullPackageJson { get; init; }
        public required PackageJsonDependencies OriginalPackageJsonDependencies {  get; init; }
        public PackageJsonDependencies? PotentialNewPackageJsonDependencies { get; set; }
    }
}
