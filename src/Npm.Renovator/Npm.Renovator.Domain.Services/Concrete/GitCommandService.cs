using Microsoft.Extensions.Logging;
using Npm.Renovator.Domain.Models;
using Npm.Renovator.Domain.Services.Abstract;

namespace Npm.Renovator.Domain.Services.Concrete
{
    internal class GitCommandService: IGitCommandService
    {
        private const string _tempFolderLocalLocation = "TestGitFolders";
        private readonly ILogger<GitCommandService> _logger;
        public GitCommandService(ILogger<GitCommandService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// IMPORTANT HTTP(s) ONLY NO SSH OR OTHERS
        /// </summary>
        public async Task<GitCommandResult<Guid>> CheckoutRemoteRepoToLocalTempStoreAsync(Uri remoteRepoLocation, CancellationToken token = default)
        {
            
            
            throw new NotImplementedException();

        }
    }
}
