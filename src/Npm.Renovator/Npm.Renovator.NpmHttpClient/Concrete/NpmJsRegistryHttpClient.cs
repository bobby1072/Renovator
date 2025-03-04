using BT.Common.HttpClient.Extensions;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npm.Renovator.NpmHttpClient.Abstract;
using Npm.Renovator.NpmHttpClient.Configuration;
using Npm.Renovator.NpmHttpClient.Extensions;
using Npm.Renovator.NpmHttpClient.Models.Request;
using Npm.Renovator.NpmHttpClient.Models.Response;
using Npm.Renovator.NpmHttpClient.Serializers.Abstract;

namespace Npm.Renovator.NpmHttpClient.Concrete
{
    internal class NpmJsRegistryHttpClient : INpmJsRegistryHttpClient
    {
        private readonly NpmJsRegistryHttpClientSettingsConfiguration _configurations;
        private readonly INpmJsRegistryHttpClientSerializer _jsonSerializer;
        private readonly ILogger<NpmJsRegistryHttpClient> _logger;
        public NpmJsRegistryHttpClient(IOptionsSnapshot<NpmJsRegistryHttpClientSettingsConfiguration> configurations,
            INpmJsRegistryHttpClientSerializer jsonSerilizer,
            ILogger<NpmJsRegistryHttpClient> logger)
        {
            _configurations = configurations.Value;
            _jsonSerializer = jsonSerilizer;
            _logger = logger;
        }


        public async Task<NpmJsRegistryResponse?> ExecuteAsync(NpmJsRegistryRequestBody requestBody, CancellationToken token = default)
        {
            var response = await _configurations.BaseUrl
                .AppendPathSegment("-")
                .AppendPathSegment("v1")
                .AppendPathSegment("search")
                .AppendQueryParam("text", requestBody.Text)
                .AppendQueryParam("size", requestBody.Size)
                .WithSettings(x => x.JsonSerializer = _jsonSerializer)
                .GetJsonAsync<NpmJsRegistryResponse>(_configurations, token)
                .HandleAndLogException(_logger);

            return response;
        }
    }
}
