namespace Npm.Renovator.ConsoleApp.Abstract;

public interface IConsoleApplicationService: IAsyncDisposable
{
    Task ExecuteAsync();
}