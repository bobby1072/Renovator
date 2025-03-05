using Npm.Renovator.ConsoleApp.Abstract;
using Npm.Renovator.Domain.Services.Abstract;

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