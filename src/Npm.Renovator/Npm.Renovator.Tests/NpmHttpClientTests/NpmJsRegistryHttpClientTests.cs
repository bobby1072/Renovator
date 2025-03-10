using AiTrainer.Web.TestBase;
using AutoFixture;
using Flurl.Http.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using Npm.Renovator.NpmHttpClient.Concrete;
using Npm.Renovator.NpmHttpClient.Configuration;
using Npm.Renovator.NpmHttpClient.Models.Request;
using Npm.Renovator.NpmHttpClient.Models.Response;
using Npm.Renovator.NpmHttpClient.Serializers.Concrete;

namespace Npm.Renovator.Tests.NpmHttpClientTests
{
    public class NpmJsRegistryHttpClientTests: IDisposable
    {
        private static readonly Fixture _fixture = new();
        private readonly Mock<ILogger<NpmJsRegistryHttpClient>> _mockLogger = new();
        private readonly NpmJsRegistryHttpClientSerializer _serialiser = new();
        private const string _baseURl = "http://localhost:5000";
        private readonly NpmJsRegistryHttpClientSettingsConfiguration _settings = _fixture
            .Build<NpmJsRegistryHttpClientSettingsConfiguration>()
            .With(x => x.BaseUrl, _baseURl)
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
            var mockedNpmJsRegistryBody = _fixture.Create<NpmJsRegistryRequestBody>();

            var mockedNpmJsRegistryResponse = _fixture.Create<NpmJsRegistryResponse>();

            _httpTest
                .ForCallsTo($"{_baseURl}/-/v1/search?text={mockedNpmJsRegistryBody.Text}&size={mockedNpmJsRegistryBody.Size}")
                .WithVerb(HttpMethod.Get)
                .RespondWithJson(mockedNpmJsRegistryResponse);

            //Act
            var result = await _httpClient.ExecuteAsync(mockedNpmJsRegistryBody);

            //Assert
            Assert.NotNull(result);

            _httpTest
                .ShouldHaveCalled($"{_baseURl}/-/v1/search?text={mockedNpmJsRegistryBody.Text}&size={mockedNpmJsRegistryBody.Size}")
                .WithVerb(HttpMethod.Get)
                .WithRequestJson(mockedNpmJsRegistryBody);
        }
    }
}
