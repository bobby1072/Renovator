namespace Npm.Renovator.Domain.Models;

public sealed class DependencyUpgradeBuilder
{
    public string LocalSystemFilePathToJson {  get; init; }
    private readonly Dictionary<string, string?> _packagesToUpgrade = [];
    private DependencyUpgradeBuilder(string localSystemFilePathToJson)
    {
        LocalSystemFilePathToJson = localSystemFilePathToJson;
    }

    public DependencyUpgradeBuilder AddUpgrade(string packageName, string? newVersion = null)
    {
        _packagesToUpgrade.Add(packageName, newVersion);  
        
        return this;
    }

    public KeyValuePair<string, string?>? GetUpgradeFor(string packageName)
    {
        return _packagesToUpgrade.FirstOrDefault(pair => pair.Key == packageName);
    }

    public static DependencyUpgradeBuilder Create(string localSystemFilePathToJson, params string[] packagesToUpgrade)
    {
        var newUpgrader = new DependencyUpgradeBuilder(localSystemFilePathToJson);

        foreach (var package in packagesToUpgrade)
        {
            newUpgrader.AddUpgrade(package);
        }
        
        return newUpgrader;
    }
}