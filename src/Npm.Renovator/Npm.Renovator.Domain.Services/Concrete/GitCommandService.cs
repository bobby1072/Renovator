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
            var gitCloneCommand = $"git clone {remoteRepoLocation} {pathToFolder}";

            await process.StandardInput.WriteLineAsync(gitCloneCommand);
            await process.StandardInput.WriteLineAsync($"cd {_tempFolderLocalLocation}");
            await process.StandardInput.WriteLineAsync($"cd {tempFolderId}");
            await process.StandardInput.WriteAsync("git submodule update --recursive --init");

            await process.StandardInput.FlushAsync(cancellationToken);
            process.StandardInput.Close();

            var result = await Task.WhenAll(process.StandardOutput.ReadToEndAsync(cancellationToken),
                process.StandardError.ReadToEndAsync(cancellationToken));

            await process.WaitForExitAsync(cancellationToken);

            var errorOutput = result.Last();
            if (!string.IsNullOrEmpty(errorOutput))
            {
                _logger.LogError("Git clone command failed with exception: {Error}", errorOutput);
            }

            return new ProcessCommandResult<TempRepositoryFromGit>
            {
                Output = ProcessHelper.GetInnerStandardOutput(result.First(), gitCloneCommand),
                ExceptionOutput = errorOutput,
                Data = new TempRepositoryFromGit 
                {
                    FolderId = tempFolderId,
                    FullPathTo = Path.GetFullPath(pathToFolder),
                    GitRepoLocation = remoteRepoLocation
                }
            };
        }
    }
}
