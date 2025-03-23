using Microsoft.Extensions.Hosting;
using Npm.Renovator.ConsoleApp.Abstract;
using Npm.Renovator.Domain.Services.Abstract;

namespace Npm.Renovator.ConsoleApp.Concrete
{
    internal class ConsoleRunnerService: IHostedService
    {
        private readonly IConsoleApplicationService _consoleApplicationService;
        private readonly IComputerResourceCheckerService _computerResourceCheckerService;
        public ConsoleRunnerService(IConsoleApplicationService consoleApplicationService, IComputerResourceCheckerService computerResourceCheckerService)
        {
            _consoleApplicationService = consoleApplicationService;
            _computerResourceCheckerService = computerResourceCheckerService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _computerResourceCheckerService.CheckResourcesAsync(cancellationToken);
            await _consoleApplicationService.ExecuteAsync();
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _consoleApplicationService.DisposeAsync().AsTask();
        }
    }
}
