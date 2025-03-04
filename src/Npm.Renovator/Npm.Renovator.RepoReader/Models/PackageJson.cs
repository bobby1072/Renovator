namespace Npm.Renovator.RepoReader.Models
{
    internal record PackageJson
    {
        public Dictionary<string, string> DevDependencies { get; init; } = [];
        public Dictionary<string, string> Dependencies { get; set; } = [];
    }
}
