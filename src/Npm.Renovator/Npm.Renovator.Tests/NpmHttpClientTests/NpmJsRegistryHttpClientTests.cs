using System.Net;
using AutoFixture;
using Npm.Renovator.NpmHttpClient.Concrete;
using Npm.Renovator.NpmHttpClient.Configuration;
using Npm.Renovator.NpmHttpClient.Models.Request;
using Npm.Renovator.NpmHttpClient.Models.Response;
using BT.Common.Http.TestBase;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Npm.Renovator.Tests.NpmHttpClientTests;


public sealed class NpmJsRegistryHttpClientTests
{
    private readonly Fixture _fixture = new();
    private readonly NpmJsRegistryHttpClientSettingsConfiguration _config;
    private readonly TestHttpClient _httpClient;
    private readonly NpmJsRegistryResponse _expectedResponse;

    public NpmJsRegistryHttpClientTests()
    {
        _config = new NpmJsRegistryHttpClientSettingsConfiguration
        {
            BaseUrl = "https://registry.npmjs.org"
        };

        _expectedResponse = _fixture.Create<NpmJsRegistryResponse>();
        _httpClient = new TestHttpClient(
            new TestStaticJsonHandler<NpmJsRegistryResponse>(
                _expectedResponse,
                HttpStatusCode.OK
            )
        );
    }

    [Fact]
    public async Task ExecuteAsync_Should_Build_Request_Correctly_And_Return_Data()
    {
        // Arrange
        var requestBody = new NpmJsRegistryRequestBody
        {
            Text = "express",
            Size = 10
        };

        const string expectedUri = "https://registry.npmjs.org/-/v1/search?text=express&size=10";

        var client = new NpmJsRegistryHttpClient(
            Options.Create(_config),
            new NullLogger<NpmJsRegistryHttpClient>(),
            _httpClient
        );

        // Act
        var result = await client.ExecuteAsync(requestBody);

        // Assert
        _httpClient.WasExpectedUrlCalled(expectedUri);
        _httpClient.WasExpectedHttpMethodUsed(HttpMethod.Get);

        Assert.NotNull(result);
        Assert.Equal(_expectedResponse.Total, result!.Total);
        Assert.Equal(_expectedResponse.Objects.Count, result.Objects.Count);
        Assert.Equal(_expectedResponse.Time, result.Time);
    }
}