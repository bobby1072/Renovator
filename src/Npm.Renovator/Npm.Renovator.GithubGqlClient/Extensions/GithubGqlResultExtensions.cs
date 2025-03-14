using Npm.Renovator.GithubGqlClient.Models;
using System.Text.Json;

namespace Npm.Renovator.GithubGqlClient.Extensions
{
    internal static class GithubGqlResultExtensions
    {
        public static T? TryAccessResult<T>(this GithubGqlResultGenericData genericResult, string keyName, JsonSerializerOptions? options = null) where T : class
        {
            try
            {
                var foundValue = genericResult.Data?.FirstOrDefault(x => x.Key == keyName);
                if(foundValue?.Value is null)
                {
                    return null;
                }

                return JsonSerializer.Deserialize<T>(foundValue.Value.Value, options);
            }
            catch
            {
                return null;
            }
        }
    }
}
