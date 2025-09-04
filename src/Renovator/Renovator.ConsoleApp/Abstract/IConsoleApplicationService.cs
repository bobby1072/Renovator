namespace Renovator.ConsoleApp.Abstract;

public interface IConsoleApplicationService: IAsyncDisposable
{
    Task ExecuteAsync(Action stopApplication, CancellationToken cancellationToken);
}