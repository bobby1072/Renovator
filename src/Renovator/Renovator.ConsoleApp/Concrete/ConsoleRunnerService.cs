using Microsoft.Extensions.Hosting;
using Renovator.Domain.Services.Abstract;
using Renovator.ConsoleApp.Abstract;

namespace Renovator.ConsoleApp.Concrete
{
    internal sealed class ConsoleRunnerService: IHostedService
    {
        private readonly IConsoleApplicationService _consoleApplicationService;
        private readonly IComputerResourceCheckerService _computerResourceCheckerService;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        public ConsoleRunnerService(IConsoleApplicationService consoleApplicationService, IComputerResourceCheckerService computerResourceCheckerService, IHostApplicationLifetime hostApplicationLifetime)
        {
            _consoleApplicationService = consoleApplicationService;
            _computerResourceCheckerService = computerResourceCheckerService;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _computerResourceCheckerService.CheckResourcesAsync(cancellationToken);
            await _consoleApplicationService.ExecuteAsync(_hostApplicationLifetime.StopApplication, cancellationToken);
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"{Environment.NewLine}Application exiting...{Environment.NewLine}");
            
            return _consoleApplicationService.DisposeAsync().AsTask();
        }
    }
}
