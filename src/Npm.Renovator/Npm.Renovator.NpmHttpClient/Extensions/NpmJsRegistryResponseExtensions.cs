using BT.Common.OperationTimer.Proto;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using Npm.Renovator.NpmHttpClient.Models.Response;

namespace Npm.Renovator.NpmHttpClient.Extensions
{
    internal static class NpmJsRegistryResponseExtensions
    {
        public static async Task<NpmJsRegistryResponse?> HandleAndLogException(this Task<NpmJsRegistryResponse> request, ILogger logger)
        {
            try
            {
                var (timeTaken, resp) = await OperationTimerUtils.TimeWithResultsAsync(() => request);

                logger.LogDebug("Request took {TimeTaken}ms to complete", timeTaken);


                return resp;
            } 
            catch(FlurlHttpException ex)
            {
                logger.LogError(ex, "Exception occurred during request with status code {Code}", ex.StatusCode);
            } 
            catch(Exception ex)
            {
                logger.LogError(ex, "Exception occurred during request");
            }
            return null;
        }
    }
}
