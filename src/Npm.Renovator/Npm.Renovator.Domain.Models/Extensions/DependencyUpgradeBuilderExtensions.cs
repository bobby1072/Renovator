using System.Text.RegularExpressions;

namespace Npm.Renovator.Domain.Models.Extensions;

public static class DependencyUpgradeBuilderExtensions
{
    public static string GetFolderSpaceFromFilePath(this DependencyUpgradeBuilder dependencyUpgradeBuilder)
    {
        var pattern = @"\\[^\\]+\.json$";
        var result = Regex.Replace(dependencyUpgradeBuilder.LocalSystemFilePathToJson, pattern, "");
        
        return result;
    }
}