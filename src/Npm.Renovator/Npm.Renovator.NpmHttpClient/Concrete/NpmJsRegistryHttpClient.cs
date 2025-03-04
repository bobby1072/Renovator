﻿using BT.Common.HttpClient.Extensions;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Npm.Renovator.NpmHttpClient.Abstract;
using Npm.Renovator.NpmHttpClient.Configuration;
using Npm.Renovator.NpmHttpClient.Models.Request;
using Npm.Renovator.NpmHttpClient.Models.Response;

namespace Npm.Renovator.NpmHttpClient.Concrete
{
    internal class NpmJsRegistryHttpClient : INpmJsRegistryHttpClient
    {
        private readonly NpmJsRegistryHttpClientSettingsConfiguration _configurations;

        public NpmJsRegistryHttpClient(IOptionsSnapshot<NpmJsRegistryHttpClientSettingsConfiguration> configurations)
        {
            _configurations = configurations.Value;
        }


        public async Task<NpmJsRegistryResponse> ExecuteAsync(NpmJsRegistryRequestBody requestBody, CancellationToken token = default)
        {
            var response = await _configurations.BaseUrl
                .AppendPathSegment("-")
                .AppendPathSegment("v1")
                .AppendPathSegment("search")
                .AppendQueryParam("text", requestBody.Text)
                .AppendQueryParam("size", requestBody.Size)
                .GetJsonAsync<NpmJsRegistryResponse>(_configurations, token);

            return response;
        }
    }
}
