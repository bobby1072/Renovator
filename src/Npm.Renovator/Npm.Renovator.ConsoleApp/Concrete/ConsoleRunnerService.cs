using Microsoft.Extensions.Hosting;
using Npm.Renovator.ConsoleApp.Abstract;

namespace Npm.Renovator.ConsoleApp.Concrete
{
    public class ConsoleRunnerService: IHostedService
    {
        private readonly IConsoleApplicationService _consoleApplicationService;
        public ConsoleRunnerService(IConsoleApplicationService consoleApplicationService)
        {
            _consoleApplicationService = consoleApplicationService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _consoleApplicationService.ExecuteAsync();
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _consoleApplicationService.DisposeAsync().AsTask();
        }
    }
}
