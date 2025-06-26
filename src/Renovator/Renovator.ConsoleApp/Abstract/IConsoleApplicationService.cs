namespace Renovator.ConsoleApp.Abstract;

public interface IConsoleApplicationService: IAsyncDisposable
{
    Task ExecuteAsync();
}