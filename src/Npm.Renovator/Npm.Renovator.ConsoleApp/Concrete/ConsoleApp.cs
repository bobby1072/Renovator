using Npm.Renovator.ConsoleApp.Abstract;

namespace Npm.Renovator.ConsoleApp.Concrete;

internal class ConsoleApp: IConsoleApp
{
    public Task ExecuteAsync()
    {
        return Task.CompletedTask;
    }
}