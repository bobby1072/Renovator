using Microsoft.Extensions.Logging;
using Npm.Renovator.Domain.Models;
using System.Management.Automation;

namespace Npm.Renovator.Domain.Services.Concrete
{
    internal class GitCommandService
    {
        private readonly ILogger<GitCommandService> _logger;
        public GitCommandService(ILogger<GitCommandService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// HTTP(s) ONLY NO SSH OR OTHERS
        /// </summary>
        public async Task<GitCommandResult> CheckoutRemoteRepoToLocalTempStoreAsync(Uri remoteRepoLocation, CancellationToken token = default)
        {
            using var ps = PowerShell.Create();
        }
    }
}
