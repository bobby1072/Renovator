using System.Text.Json;

namespace Npm.Renovator.Domain.Models.Extensions
{
    public static class LazyPackageJsonExtensions
    {
        public static LazyPackageJson? FindPackageJsonByPropertyWithin(this IEnumerable<LazyPackageJson> lazyPackages, (string Key, string Value) keyValue)
        {
            foreach (var lazyPack in lazyPackages)
            {
                if (lazyPack.FullPackageJson.Value.Any(x => x.Key == keyValue.Key && x.Value?.ToString() == keyValue.Value))
                {
                    return lazyPack;
                }
            }

            return null;
        }

        public static T? TryGetPropertyValueFromPackageJson<T>(this LazyPackageJson lazyPackageJson, string propertyName, JsonSerializerOptions? jsonOpts = null)
        {
            try
            {
                var stringJson = lazyPackageJson.FullPackageJson.Value.FirstOrDefault(x => x.Key == propertyName).Value?.ToString();

                if (stringJson is null)
                {
                    return default;
                }

                if (!stringJson.StartsWith('[') || !stringJson.StartsWith('{')) 
                {
                    if (typeof(T) == typeof(string)) return (T)(object)stringJson;
                    else if (typeof(T) == typeof(int) && int.TryParse(stringJson, out var intValue)) return (T)(object)intValue;
                    else if (typeof(T) == typeof(bool) && bool.TryParse(stringJson, out var boolValue)) return (T)(object)boolValue;
                    else if (typeof(T) == typeof(double) && double.TryParse(stringJson, out var doubleValue)) return (T)(object)doubleValue;
                }
                return JsonSerializer.Deserialize<T>(stringJson, jsonOpts);
            }
            catch
            {
                return default;
            }
        } 
        public static IEnumerable<T?> TryGetPropertyFromPackageJson<T>(this IEnumerable<LazyPackageJson> lazyPackageJsons, string propertyName, JsonSerializerOptions? jsonOpts = null)
        {
            foreach(var pack in lazyPackageJsons)
            {
                yield return TryGetPropertyValueFromPackageJson<T>(pack, propertyName, jsonOpts);
            }
        }
    }
}
