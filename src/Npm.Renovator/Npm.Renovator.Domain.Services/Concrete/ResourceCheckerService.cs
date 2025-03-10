using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npm.Renovator.Common.Helpers;
using System.Diagnostics;

namespace Npm.Renovator.Domain.Services.Concrete
{
    public class ResourceCheckerService: IHostedService
    {
        private readonly ILogger<ResourceCheckerService> _logger;

        public ResourceCheckerService(ILogger<ResourceCheckerService> logger)
        {
            _logger = logger;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var process = new Process();
                process.StartInfo = ProcessHelper.GetProcessStartInfo();
                process.Start();

                await process.StandardInput.WriteLineAsync("npm -v");
                await process.StandardInput.WriteLineAsync("node -v");

                await process.StandardInput.FlushAsync(cancellationToken);
                process.StandardInput.Close();

                var result = await Task.WhenAll(process.StandardOutput.ReadToEndAsync(cancellationToken),
                    process.StandardError.ReadToEndAsync(cancellationToken));

                await process.WaitForExitAsync(cancellationToken);

                if (!string.IsNullOrEmpty(result.Last()))
                {
                    throw new InvalidProgramException("This system does not have the resources required to run the app...");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during execution of ResourceCheckerService");

                throw new InvalidProgramException("This system does not have the resources required to run the app...");
            }
        }
        public Task StopAsync(CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}