using Microsoft.Extensions.Logging;
using Npm.Renovator.Domain.Models;
using System.Management.Automation;

namespace Npm.Renovator.Domain.Services.Concrete
{
    internal class GitCommandService
    {
        private const string _tempFolderLocalLocation = "TestGitFolders";
        private readonly ILogger<GitCommandService> _logger;
        public GitCommandService(ILogger<GitCommandService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// HTTP(s) ONLY NO SSH OR OTHERS
        /// </summary>
        public async Task<GitCommandResult<Guid>> CheckoutRemoteRepoToLocalTempStoreAsync(Uri remoteRepoLocation, CancellationToken token = default)
        {
            using var ps = PowerShell.Create();
            var tempRepoId = Guid.NewGuid();
            
            ps.AddScript($"git clone {remoteRepoLocation} {Path.Combine(".", _tempFolderLocalLocation, tempRepoId.ToString())}");
            
            ps.AddScript($"cd {_tempFolderLocalLocation}");
            ps.AddScript($"cd {tempRepoId}");

            var result = await ps.InvokeAsync();

            throw new NotImplementedException();
        }
    }
}
