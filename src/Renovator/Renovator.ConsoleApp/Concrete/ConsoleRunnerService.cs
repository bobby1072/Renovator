using Microsoft.Extensions.Hosting;
using Renovator.Domain.Services.Abstract;
using Renovator.ConsoleApp.Abstract;

namespace Renovator.ConsoleApp.Concrete
{
    internal sealed class ConsoleRunnerService: IHostedService
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
