using System.Text.Json.Serialization;

namespace Npm.Renovator.RepoReader.Models
{
    public record PackageJsonDependencies
    {
        [JsonPropertyName("devDependencies")]
        public Dictionary<string, string> DevDependencies { get; init; } = [];
        [JsonPropertyName("dependencies")]
        public Dictionary<string, string> Dependencies { get; init; } = [];
    }
}
