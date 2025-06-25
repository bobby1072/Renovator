using BT.Common.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npm.Renovator.NpmHttpClient.Abstract;
using Npm.Renovator.NpmHttpClient.Configuration;
using Npm.Renovator.NpmHttpClient.Models.Request;
using Npm.Renovator.NpmHttpClient.Models.Response;

namespace Npm.Renovator.NpmHttpClient.Concrete
{
    internal sealed class NpmJsRegistryHttpClient : INpmJsRegistryHttpClient
    {
        private readonly NpmJsRegistryHttpClientSettingsConfiguration _configurations;
        private readonly ILogger<NpmJsRegistryHttpClient> _logger;
        private readonly HttpClient _httpClient;
        public NpmJsRegistryHttpClient(IOptions<NpmJsRegistryHttpClientSettingsConfiguration> configurations,
            ILogger<NpmJsRegistryHttpClient> logger, HttpClient httpClient)
        {
            _configurations = configurations.Value;
            _logger = logger;
            _httpClient = httpClient;
        }


        public async Task<NpmJsRegistryResponse?> ExecuteAsync(NpmJsRegistryRequestBody requestBody, CancellationToken token = default)
        {
            var response = await _configurations.BaseUrl
                .AppendPathSegment("-")
                .AppendPathSegment("v1")
                .AppendPathSegment("search")
                .AppendQueryParameter("text", requestBody.Text)
                .AppendQueryParameter("size", requestBody.Size.ToString())
                .GetJsonAsync<NpmJsRegistryResponse>(_httpClient, token);

            return response;
        }
    }
}
