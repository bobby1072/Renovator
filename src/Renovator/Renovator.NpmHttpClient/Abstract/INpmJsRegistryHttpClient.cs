using Renovator.NpmHttpClient.Models.Request;
using Renovator.NpmHttpClient.Models.Response;

namespace Renovator.NpmHttpClient.Abstract
{
    public interface INpmJsRegistryHttpClient
    {
        Task<NpmJsRegistryResponse?> ExecuteAsync(NpmJsRegistryRequestBody requestBody, CancellationToken token = default);
    }
}
