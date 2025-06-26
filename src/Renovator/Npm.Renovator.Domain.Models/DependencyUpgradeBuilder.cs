using Npm.Renovator.Common.Extensions;

namespace Npm.Renovator.Domain.Models
{
    public abstract class DependencyUpgradeBuilder
    {
        public IReadOnlyDictionary<string, string?> ReadonlyUpgradesView
        {
            get => _packagesToUpgrade.Clone();
        }
        private readonly Dictionary<string, string?> _packagesToUpgrade = [];

        public bool HasAnyUpgrades() => _packagesToUpgrade.Count != 0;

        public DependencyUpgradeBuilder AddUpgrade(string packageName, string? newVersion = null)
        {
            _packagesToUpgrade.Add(packageName, newVersion);

            return this;
        }

        public KeyValuePair<string, string?>? GetUpgradeFor(string packageName)
        {
            return _packagesToUpgrade.FirstOrDefault(pair => pair.Key == packageName);
        }
    }
}
