using System.Text;
using Npm.Renovator.ConsoleApp.Abstract;
using Npm.Renovator.Domain.Services.Abstract;

namespace Npm.Renovator.ConsoleApp.Concrete;

internal class ConsoleApplicationService: IConsoleApplicationService
{
    private readonly INpmRenovatorProcessingManager _processingManager;
    public ConsoleApplicationService(INpmRenovatorProcessingManager processingManager)
    {
        _processingManager = processingManager;
    }
    public async Task ExecuteAsync()
    {
        try
        {
            var cancelTokenSource = new CancellationTokenSource();
            Task.Run(() =>
            {
                while (true)
                {
                    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                    {
                        cancelTokenSource.Cancel();
                        break;
                    }

                    Thread.Sleep(100);
                }
            });
            while (!cancelTokenSource.IsCancellationRequested)
            {
                
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"{NewConsoleLines(2)}Operation has been cancelled...{NewConsoleLines(2)}");
        }
        catch (Exception)
        {
            Console.WriteLine($"{NewConsoleLines(2)}An unexpected exception occurred...{NewConsoleLines(2)}");

            await ExecuteAsync();
        }
    }

    private async Task MainMenu()
    {
        
    }
    private static string NewConsoleLines(int numberOf = 1)
    {
        var newLineBuilder = new StringBuilder();
        for (int i = 0; i < numberOf; i++)
        {
            newLineBuilder.Append(Environment.NewLine);
        }
        
        return newLineBuilder.ToString();
    }

    private enum MainMenuChoice
    {
        
    }
}