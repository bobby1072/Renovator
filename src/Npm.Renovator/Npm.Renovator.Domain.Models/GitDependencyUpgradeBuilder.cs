namespace Npm.Renovator.Domain.Models
{
    public class GitDependencyUpgradeBuilder: DependencyUpgradeBuilder
    {
        public Uri RemoteRepoLocation { get; init; }
        /// <summary>
        /// Should reflect the 'name' property in your target package.json
        /// </summary>
        public string NameInPackageJson { get; init; }
        private GitDependencyUpgradeBuilder(Uri remoteRepoLocation, string localSystemFilePathToJson)
        {
            RemoteRepoLocation = remoteRepoLocation;
            NameInPackageJson = localSystemFilePathToJson;
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
