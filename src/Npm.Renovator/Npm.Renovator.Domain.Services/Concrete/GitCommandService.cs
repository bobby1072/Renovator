using Microsoft.Extensions.Logging;
using Npm.Renovator.Common.Helpers;
using Npm.Renovator.Domain.Models;
using Npm.Renovator.Domain.Services.Abstract;
using System.Diagnostics;
using System.Threading;

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
        public async Task<ProcessCommandResult<TempRepositoryFromGit>> CheckoutRemoteRepoToLocalTempStoreAsync(Uri remoteRepoLocation, CancellationToken cancellationToken = default)
        {
            using var process = new Process();
            process.StartInfo = ProcessHelper.GetDefaultProcessStartInfo();
            var tempFolderId = Guid.NewGuid();
            var pathToFolder = Path.Combine(".", _tempFolderLocalLocation, tempFolderId.ToString());
            process.Start();

            await process.StandardInput.WriteLineAsync($"git clone {remoteRepoLocation} {pathToFolder}");

            await process.StandardInput.FlushAsync(cancellationToken);
            process.StandardInput.Close();

            var result = await Task.WhenAll(process.StandardOutput.ReadToEndAsync(cancellationToken),
                process.StandardError.ReadToEndAsync(cancellationToken));

            await process.WaitForExitAsync(cancellationToken);


            throw new NotImplementedException();
        }
    }
}
