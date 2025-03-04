using Npm.Renovator.NpmHttpClient.Serializers.Abstract;
using System.Text.Json;

namespace Npm.Renovator.NpmHttpClient.Serializers.Concrete
{
    internal class NpmJsRegistryHttpClientSerializer : INpmJsRegistryHttpClientSerializer
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        public string Serialize(object obj) => JsonSerializer.Serialize(obj, _jsonOptions);
        public T Deserialize<T>(string s) => JsonSerializer.Deserialize<T>(s, _jsonOptions)
            ?? throw new JsonException($"Failed to deserialize json to {typeof(T).Name}");
        public T Deserialize<T>(Stream stream) => JsonSerializer.Deserialize<T>(stream, _jsonOptions)
            ?? throw new JsonException($"Failed to deserialize json to {typeof(T).Name}");
    }
}
