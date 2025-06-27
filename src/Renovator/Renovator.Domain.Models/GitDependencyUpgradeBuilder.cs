namespace Renovator.Domain.Models
{
    public sealed class GitDependencyUpgradeBuilder: DependencyUpgradeBuilder
    {
        public Uri RemoteRepoLocation { get; }
        /// <summary>
        /// Should reflect the 'name' property in your target package.json
        /// </summary>
        public string NameInPackageJson { get; }
        private GitDependencyUpgradeBuilder(Uri remoteRepoLocation, string nameInPackageJson)
        {
            RemoteRepoLocation = remoteRepoLocation;
            NameInPackageJson = nameInPackageJson;
        }
        public static GitDependencyUpgradeBuilder Create(Uri remoteRepoLocation, string localSystemFilePathToJson, params string[] packagesToUpgrade)
        {
            var newUpgrader = new GitDependencyUpgradeBuilder(remoteRepoLocation, localSystemFilePathToJson);

            foreach (var package in packagesToUpgrade)
            {
                newUpgrader.AddUpgrade(package);
            }

            return newUpgrader;
        }
    }
}
