using System.Text.Json.Serialization;

namespace Npm.Renovator.RepoReader.Models
{
    public record PackageJsonDependencies
    {
        public Dictionary<string, string> DevDependencies { get; init; } = [];
        public Dictionary<string, string> Dependencies { get; init; } = [];
    }
}
