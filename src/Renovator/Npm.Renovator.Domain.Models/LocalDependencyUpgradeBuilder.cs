namespace Npm.Renovator.Domain.Models;

public sealed class LocalDependencyUpgradeBuilder: DependencyUpgradeBuilder
{
    public string LocalSystemFilePathToJson { get; init; }
    private LocalDependencyUpgradeBuilder(string localSystemFilePathToJson)
    {
        LocalSystemFilePathToJson = localSystemFilePathToJson;
    }
    public static LocalDependencyUpgradeBuilder Create(string localSystemFilePathToJson, params (string Name, string Version)[] packagesToUpgrade)
    {
        var newUpgrader = new LocalDependencyUpgradeBuilder(localSystemFilePathToJson);

        foreach (var package in packagesToUpgrade)
        {
            newUpgrader.AddUpgrade(package.Name, package.Version);
        }

        return newUpgrader;
    }
}