using System.Text;
using System.Xml;
using Npm.Renovator.ConsoleApp.Abstract;
using Npm.Renovator.ConsoleApp.Exception;
using Npm.Renovator.ConsoleApp.Models;
using Npm.Renovator.Domain.Models;
using Npm.Renovator.Domain.Models.Views;
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
            Console.WriteLine($"{NewConsoleLines()}Operation has been cancelled...{NewConsoleLines()}");
        }
        catch(ConsoleException ex)
        {
            Console.WriteLine(ex.Message);
            await Task.Delay(5000);
            await ExecuteAsync();
        }
        catch (System.Exception)
        {
            Console.WriteLine($"{NewConsoleLines()}An unexpected exception occurred...{NewConsoleLines()}");
            await Task.Delay(5000);
            await ExecuteAsync();
        }
    }

    private Task<ConsoleJourneyState> MainMenuJourney(CancellationToken cancellationToken)
    {
        Console.Clear();
        Console.WriteLine($"{NewConsoleLines()}Welcome to the Npm.Renovator.ConsoleApp...{NewConsoleLines()}");
        Console.WriteLine($"This app can be used to renovate your Node.JS projects.{NewConsoleLines(2)}");
        Console.WriteLine($"1. View potential package upgrades {NewConsoleLines()}");
        Console.WriteLine($"2. Attempt to renovate project within your local file system. {NewConsoleLines(2)}");
        Console.WriteLine($"Please choose an option: {NewConsoleLines()}");
        
        var consoleChoice = Console.ReadLine();

        if (consoleChoice != "1" && consoleChoice != "2")
        {
            throw new ConsoleException($"{NewConsoleLines()}Please choose a valid option.{NewConsoleLines()}");
        }
        
        Console.WriteLine($"{NewConsoleLines(2
            )}Please enter the local file system path to your package json: {NewConsoleLines()}");
        
        var localFilePath = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(localFilePath) || string.IsNullOrEmpty(localFilePath))
        {
            throw new ConsoleException($"{NewConsoleLines()}Please enter a valid file system path.{NewConsoleLines()}");
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

    private async Task<ConsoleJourneyState> GetCurrentPackageVersionAndPotentialUpgradesViewJourney(DependencyUpgradeBuilder upgradeBuilder, CancellationToken token)
    {
        Console.Clear();
        var potentialUpgradesViewJob = _processingManager
            .GetCurrentPackageVersionAndPotentialUpgradesViewForLocalSystemRepoAsync(
                upgradeBuilder.LocalSystemFilePathToJson, token);
        Console.WriteLine($"{NewConsoleLines()}Getting view. Please wait...{NewConsoleLines()}");
        
        var potentialUpgradesView = await potentialUpgradesViewJob;

        if (!potentialUpgradesView.IsSuccess || potentialUpgradesView.Data is null)
        {
            throw new ConsoleException($"{NewConsoleLines()}Failed to retrieve potential upgrades view.{NewConsoleLines()}");
        }
        
        DisplayCurrentPackageVersionsAndPotentialUpgradesView(potentialUpgradesView.Data);


        Console.WriteLine($"{NewConsoleLines(2)}Do you want to return to the main menu (y/n)?{NewConsoleLines()}");

        var choice = Console.ReadLine();

        if(choice == "n")
        {
            throw new OperationCanceledException();
        }

        return new ConsoleJourneyState { NextMove = MainMenuJourney };
        
    }
    private static Task<ConsoleJourneyState> AttemptToRenovateRepoJourney(DependencyUpgradeBuilder upgradeBuilder, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    private static void DisplayCurrentPackageVersionsAndPotentialUpgradesView(
        CurrentPackageVersionsAndPotentialUpgradesView view)
    {
        foreach (var upg in view.AllPackages)
        {
            Console.WriteLine($"Package name: {upg.NameOnNpm}");
            Console.WriteLine($"Current version: {upg.CurrentVersion}");
            if(upg.PotentialNewVersions.Count < 1)
            {
                Console.WriteLine(NewConsoleLines());
                continue;
            }
            Console.WriteLine($"Potential upgrades:");
            foreach (var newPackage in upg.PotentialNewVersions)
            {
                Console.WriteLine($"    Version: {newPackage.CurrentVersion}");
                Console.WriteLine($"    Published: {newPackage.ReleaseDate}");
            }

            Console.WriteLine(NewConsoleLines());
        }
    }
    private static string GetYNChoice()
    {
        var choice = Console.ReadLine()?.ToLower();

        if(choice != "n" && choice != "y")
        {
            throw new ConsoleException($"{NewConsoleLines()}Please enter y or n...{NewConsoleLines()}");
        }

        return choice;
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