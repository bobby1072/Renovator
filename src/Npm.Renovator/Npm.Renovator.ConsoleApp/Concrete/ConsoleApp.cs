using Npm.Renovator.Application.Services.Abstract;
using Npm.Renovator.ConsoleApp.Abstract;

namespace Npm.Renovator.ConsoleApp.Concrete;

internal class ConsoleApp: IConsoleApp
{
    private readonly INpmRenovatorProcessingManager _processingManager;
    public ConsoleApp(INpmRenovatorProcessingManager processingManager)
    {
        _processingManager = processingManager;
    }
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
    }
}