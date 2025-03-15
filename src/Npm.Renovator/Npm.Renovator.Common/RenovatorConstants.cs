using System.Text.Json;
using System.Text.Json.Nodes;

namespace Npm.Renovator.Common
{
    public static class RenovatorConstants
    {
        public static readonly JsonSerializerOptions CamelCaseSerializerOptions =
            new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        public static readonly JsonNodeOptions JsonNodeOptionsPropertyNameCaseInsensitive =
            new() { PropertyNameCaseInsensitive = true };
    }
}
