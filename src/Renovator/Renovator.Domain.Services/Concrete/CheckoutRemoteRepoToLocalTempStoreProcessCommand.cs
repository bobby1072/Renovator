using Microsoft.Extensions.Logging;
using Renovator.Common.Helpers;
using Renovator.Domain.Models;
using System.Diagnostics;
using Renovator.Domain.Services.Abstract;

namespace Renovator.Domain.Services.Concrete
{
    internal sealed class CheckoutRemoteRepoToLocalTempStoreProcessCommand: BaseProcessCommand<Uri, ProcessCommandResult<TempRepositoryFromGit>>
    {
        public override string Name => nameof(CheckoutRemoteRepoToLocalTempStoreProcessCommand);
        private const string _tempFolderLocalLocation = "TestGitFolders";
        private readonly ILogger<CheckoutRemoteRepoToLocalTempStoreProcessCommand> _logger;
        public CheckoutRemoteRepoToLocalTempStoreProcessCommand(Process process, ILogger<CheckoutRemoteRepoToLocalTempStoreProcessCommand> logger)
            : base(process)
        {
            _logger = logger;
        }

        /// <summary>
        /// IMPORTANT HTTP(s) ONLY NO SSH OR OTHERS
        /// </summary>
        public override async Task<ProcessCommandResult<TempRepositoryFromGit>> ExecuteCommandAsync(Uri remoteRepoLocation, CancellationToken cancellationToken = default)
        {
            _processRunner.StartInfo = ProcessHelper.GetDefaultProcessStartInfo();
            var tempFolderId = Guid.NewGuid();
            var pathToFolder = Path.Combine(".", _tempFolderLocalLocation, tempFolderId.ToString());
            _processRunner.Start();
            var gitCloneCommand = $"git clone {remoteRepoLocation} {pathToFolder}";

            await _processRunner.StandardInput.WriteLineAsync(gitCloneCommand);
            await _processRunner.StandardInput.WriteLineAsync($"cd {_tempFolderLocalLocation}");
            await _processRunner.StandardInput.WriteLineAsync($"cd {tempFolderId}");
            await _processRunner.StandardInput.WriteAsync("git submodule update --recursive --init");

            await _processRunner.StandardInput.FlushAsync(cancellationToken);
            _processRunner.StandardInput.Close();

            var result = await Task.WhenAll(_processRunner.StandardOutput.ReadToEndAsync(cancellationToken),
                _processRunner.StandardError.ReadToEndAsync(cancellationToken));

            await _processRunner.WaitForExitAsync(cancellationToken);

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
