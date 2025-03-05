using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Npm.Renovator.Common.Extensions;

public static class JsonObjectExtensions
{
    public static JsonObject UpdateProperties<T>(this JsonObject jsonObject, T objectToUpdateWith, JsonSerializerOptions? options = null) where T : class
    {
        var typeProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in typeProperties)
        {
            var propertyName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;
                
            jsonObject[propertyName] = JsonSerializer.Serialize(property.GetValue(objectToUpdateWith), options);
        }
            
        return jsonObject;
    }
}