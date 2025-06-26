using System.Text.Json;

namespace Renovator.Common
{
    public static class RenovatorConstants
    {
        public static readonly JsonSerializerOptions DefaultCamelCaseJsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
    }
}