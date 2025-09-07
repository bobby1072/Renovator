using Microsoft.Extensions.Hosting;
using Renovator.ConsoleApp.Abstract;
using Renovator.Domain.Models;
using Renovator.Domain.Services.Abstract;
using Renovator.Domain.Services.Concrete;

namespace Renovator.ConsoleApp.Concrete
{
    internal sealed class ConsoleRunnerService: IHostedService
    {
        private readonly IConsoleApplicationService _consoleApplicationService;
        private readonly IProcessExecutor _processExecutor;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        public ConsoleRunnerService(IConsoleApplicationService consoleApplicationService,
            IProcessExecutor processExecutor,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            _consoleApplicationService = consoleApplicationService;
            _processExecutor = processExecutor;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _processExecutor.RunCommandAsync<ComputerResourceCheckProcessCommand, ProcessCommandResult>(
                cancellationToken);
            await _consoleApplicationService.ExecuteAsync(_hostApplicationLifetime.StopApplication, cancellationToken);
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"{Environment.NewLine}Application exiting...{Environment.NewLine}");
            
            return _consoleApplicationService.DisposeAsync().AsTask();
        }
    }
}
