using Microsoft.Extensions.Logging;
using Npm.Renovator.Domain.Models;

namespace Npm.Renovator.Domain.Services.Concrete
{
    internal class GitCommandService
    {
        private readonly ILogger<GitCommandService> _logger;
        public GitCommandService(ILogger<GitCommandService> logger)
        {
            _logger = logger;
        }


        //public async Task<GitCommandResult> CheckoutRemoteRepoToLocalTempStoreAsync(,CancellationToken token = default)
        //{

        //}
    }
}
