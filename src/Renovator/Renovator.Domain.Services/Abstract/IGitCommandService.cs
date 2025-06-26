using Renovator.Domain.Models;

namespace Renovator.Domain.Services.Abstract
{
    internal interface IGitCommandService
    {
        /// <summary>
        /// IMPORTANT HTTP(s) ONLY NO SSH OR OTHERS
        /// </summary>
        Task<ProcessCommandResult<TempRepositoryFromGit>> CheckoutRemoteRepoToLocalTempStoreAsync(Uri remoteRepoLocation, CancellationToken token = default);
    }
}
