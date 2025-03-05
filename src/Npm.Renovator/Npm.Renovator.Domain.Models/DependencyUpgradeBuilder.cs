namespace Npm.Renovator.Domain.Models;

public sealed class DependencyUpgradeBuilder
{
    private string _filePath;
    public string FilePath
    {
        get => _filePath;
        set => _filePath = Path.GetFullPath(value);
    }
    private readonly Dictionary<string, string?> _packagesToUpgrade = [];

    public DependencyUpgradeBuilder AddUpgrade(string packageName, string? newVersion = null)
    {
        _packagesToUpgrade.Add(packageName, newVersion);  
        
        return this;
    }

    public KeyValuePair<string, string?>? GetUpgradeFor(string packageName)
    {
        return _packagesToUpgrade.FirstOrDefault(pair => pair.Key == packageName);
    }

    private DependencyUpgradeBuilder(string filePath)
    {
        _filePath = filePath;
    }
    public static DependencyUpgradeBuilder Create(string filePath, params string[] packagesToUpgrade)
    {
        var newUpgrader = new DependencyUpgradeBuilder(filePath);

        foreach (var package in packagesToUpgrade)
        {
            newUpgrader.AddUpgrade(package);
        }
        
        return newUpgrader;
    }
}