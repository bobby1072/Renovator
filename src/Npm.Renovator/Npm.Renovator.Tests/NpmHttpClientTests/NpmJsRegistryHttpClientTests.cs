using AutoFixture;
using Flurl.Http.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using Npm.Renovator.NpmHttpClient.Concrete;
using Npm.Renovator.NpmHttpClient.Configuration;
using Npm.Renovator.NpmHttpClient.Models.Request;
using Npm.Renovator.NpmHttpClient.Models.Response;
using System.Text.Json;

namespace Npm.Renovator.Tests.NpmHttpClientTests
{
    public class NpmJsRegistryHttpClientTests: IDisposable
    {
        private static readonly JsonSerializerOptions _serialiserOpts = new (){ PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private static readonly Fixture _fixture = new();
        private readonly Mock<ILogger<NpmJsRegistryHttpClient>> _mockLogger = new();
        private readonly NpmJsRegistryHttpClientSerializer _serialiser = new();
        private const string _baseURl = "http://localhost:5000";
        private readonly NpmJsRegistryHttpClientSettingsConfiguration _settings = _fixture
            .Build<NpmJsRegistryHttpClientSettingsConfiguration>()
            .With(x => x.BaseUrl, _baseURl)
            .With(x => x.DelayBetweenAttemptsInSeconds, 0)
            .With(x => x.TimeoutInSeconds, 2)
            .With(x => x.TotalAttempts, 1)
            .Create();
        private readonly NpmJsRegistryHttpClient _httpClient;
        private readonly HttpTest _httpTest = new();
        public NpmJsRegistryHttpClientTests() 
        {
            _httpClient = new NpmJsRegistryHttpClient(
                new TestOptionsSnapshot<NpmJsRegistryHttpClientSettingsConfiguration>(_settings).Object,
                _serialiser,
                _mockLogger.Object
            );
        }
        public void Dispose()
        {
            _httpTest.Dispose();
        }




        [Fact]
        public async Task Client_Builds_Request_Correctly()
        {
            //Arrange
            var mockedNpmJsRegistryBody = _fixture
                .Build<NpmJsRegistryRequestBody>()
                .With(x => x.Text, "zod")
                .Create();

            var mockedNpmJsRegistryResponse = _fixture.Create<NpmJsRegistryResponse>();

            _httpTest
                .ForCallsTo($"{_baseURl}/-/v1/search?text={mockedNpmJsRegistryBody.Text}&size={mockedNpmJsRegistryBody.Size}")
                .WithVerb(HttpMethod.Get)
                .RespondWith(JsonSerializer.Serialize(mockedNpmJsRegistryResponse, _serialiserOpts));

            //Act
            var result = await _httpClient.ExecuteAsync(mockedNpmJsRegistryBody);

            //Assert
            Assert.NotNull(result);

            _httpTest
                .ShouldHaveCalled($"{_baseURl}/-/v1/search?text={mockedNpmJsRegistryBody.Text}&size={mockedNpmJsRegistryBody.Size}")
                .WithVerb(HttpMethod.Get);
        }
    }
}
