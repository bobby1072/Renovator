using Microsoft.Extensions.Logging;
using Npm.Renovator.Common.Helpers;
using Npm.Renovator.Domain.Services.Abstract;
using System.Diagnostics;

namespace Npm.Renovator.Domain.Services.Concrete
{
    internal sealed class ComputerResourceCheckerService: IComputerResourceCheckerService
    {
        private readonly ILogger<ComputerResourceCheckerService> _logger;

        public ComputerResourceCheckerService(ILogger<ComputerResourceCheckerService> logger)
        {
            _logger = logger;
        }
        public async Task CheckResourcesAsync(CancellationToken token)
        {
            try
            {
                using var process = new Process();
                process.StartInfo = ProcessHelper.GetDefaultProcessStartInfo();
                process.Start();

                await process.StandardInput.WriteLineAsync("npm -v");
                await process.StandardInput.WriteLineAsync("node -v");
                await process.StandardInput.WriteLineAsync("git -v");

                await process.StandardInput.FlushAsync(token);
                process.StandardInput.Close();

                var result = await Task.WhenAll(process.StandardOutput.ReadToEndAsync(token),
                    process.StandardError.ReadToEndAsync(token));

                await process.WaitForExitAsync(token);

                var errors = result.Last();

                if (!string.IsNullOrEmpty(errors))
                {
                    _logger.LogError("Exception occurred during execution of ResourceCheckerService. Errors: {Errors}", errors);

                    throw new InvalidProgramException("This system does not have the resources required to run the app...");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during execution of ResourceCheckerService");

                throw new InvalidProgramException("This system does not have the resources required to run the app...");
            }
        }
    }
}