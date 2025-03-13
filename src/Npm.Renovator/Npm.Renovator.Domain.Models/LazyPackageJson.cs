using System.Text.Json.Nodes;

namespace Npm.Renovator.Domain.Models
{
    public record LazyPackageJson
    {
        public required string FullLocalPathToPackageJson { get; init; }
        public required JsonObject FullPackageJson { get; init; }
        public required PackageJsonDependencies OriginalPackageJsonDependencies {  get; init; }
        public PackageJsonDependencies? PotentialNewPackageJsonDependencies { get; set; }
    }
}
