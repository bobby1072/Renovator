using System.Text;
using Npm.Renovator.ConsoleApp.Abstract;
using Npm.Renovator.ConsoleApp.Models;
using Npm.Renovator.Domain.Models;
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

            var consoleJourneyState = new ConsoleJourneyState
            {
                NextMove = MainMenuJourney
            };
            while (!cancelTokenSource.IsCancellationRequested)
            {
                consoleJourneyState = await consoleJourneyState.NextMove.Invoke(cancelTokenSource.Token);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"{NewConsoleLines(2)}Operation has been cancelled...{NewConsoleLines(2)}");
        }
        catch (System.Exception)
        {
            Console.WriteLine($"{NewConsoleLines(2)}An unexpected exception occurred...{NewConsoleLines(2)}");
            await Task.Delay(1000);
            await ExecuteAsync();
        }
    }

    private static Task<ConsoleJourneyState> MainMenuJourney(CancellationToken cancellationToken)
    {
        Console.Clear();
        Console.WriteLine($@"{NewConsoleLines()}Welcome to the Npm.Renovator.ConsoleApp...{NewConsoleLines()}
            This app can be used to renovate your Node.JS projects.{NewConsoleLines(2)}
            1. View potential package upgrades {NewConsoleLines()}
            2. Attempt to renovate project within your local file system. {NewConsoleLines(2)} 
            Please choose an option: {NewConsoleLines()}
        ");
        
        var consoleChoice = Console.ReadLine();

        if (consoleChoice != "1" || consoleChoice != "2")
        {
            throw new ArgumentException($"{NewConsoleLines()}Please choose a valid option.{NewConsoleLines()}");
        }
        
        Console.WriteLine($"{NewConsoleLines(2
            )}Please enter the local file system path to your package json: {NewConsoleLines()}");
        
        var localFilePath = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(localFilePath) || string.IsNullOrEmpty(localFilePath))
        {
            throw new ArgumentException($"{NewConsoleLines()}Please enter a valid file system path.{NewConsoleLines()}");
        }        
        
        var upgradeBuilder = DependencyUpgradeBuilder.Create(localFilePath);

        return Task.FromResult(new ConsoleJourneyState
        {
            NextMove = ct =>
                consoleChoice == "1"
                    ? GetCurrentPackageVersionAndPotentialUpgradesViewJourney(upgradeBuilder, ct)
                    : AttemptToRenovateRepoJourney(upgradeBuilder, ct)
        });
    }

    private static Task<ConsoleJourneyState> AttemptToRenovateRepoJourney(DependencyUpgradeBuilder upgradeBuilder, CancellationToken token)
    {
        throw new NotImplementedException();
    }
    private static Task<ConsoleJourneyState> GetCurrentPackageVersionAndPotentialUpgradesViewJourney(DependencyUpgradeBuilder upgradeBuilder, CancellationToken token)
    {
        throw new NotImplementedException();
    }
    private static string NewConsoleLines(int numberOf = 1)
    {
        if(numberOf == 1) return Environment.NewLine;
        var newLineBuilder = new StringBuilder();
        for (int i = 0; i < numberOf; i++)
        {
            newLineBuilder.Append(Environment.NewLine);
        }
        
        return newLineBuilder.ToString();
    }
}