using Npm.Renovator.NpmHttpClient.Models.Request;
using Npm.Renovator.NpmHttpClient.Models.Response;

namespace Npm.Renovator.NpmHttpClient.Abstract
{
    public interface INpmJsRegistryHttpClient
    {
        Task<NpmJsRegistryResponse> ExecuteAsync(NpmJsRegistryRequestBody requestBody, CancellationToken token = default);
    }
}
