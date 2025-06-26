using System.Text.RegularExpressions;

namespace Npm.Renovator.Common.Extensions
{
    public static class StringExtensions
    {
        public static string GetFolderSpaceFromFilePath(this string localFilePathToJson)
        {
            var pattern = @"\\[^\\]+\.json$";
            var result = Regex.Replace(Path.GetFullPath(localFilePathToJson), pattern, "");

            return result;
        }
    }
}