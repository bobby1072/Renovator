using BT.Common.FastArray.Proto;
using Microsoft.Extensions.DependencyInjection;
using Npm.Renovator.Common.Extensions;
using Npm.Renovator.ConsoleApp.Abstract;
using Npm.Renovator.ConsoleApp.Exception;
using Npm.Renovator.ConsoleApp.Models;
using Npm.Renovator.Domain.Models;
using Npm.Renovator.Domain.Models.Views;
using Npm.Renovator.Domain.Services.Abstract;
using System.Text;

namespace Npm.Renovator.ConsoleApp.Concrete;

internal class ConsoleApplicationService : IConsoleApplicationService
{
    private readonly IServiceProvider _serviceProvider;
    private AsyncServiceScope _asyncScope;
    private INpmRenovatorProcessingManager _processingManager { get => _asyncScope.ServiceProvider.GetRequiredService<INpmRenovatorProcessingManager>(); }
    private IGitNpmRenovatorProcessingManager _gitProcessingManager { get => _asyncScope.ServiceProvider.GetRequiredService<IGitNpmRenovatorProcessingManager>(); }
    public ConsoleApplicationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public ValueTask DisposeAsync() => _asyncScope.DisposeAsync();
    public async Task ExecuteAsync()
    {
        while (true)
        {
            try
            {
                _asyncScope = _serviceProvider.CreateAsyncScope();

                var cancelTokenSource = new CancellationTokenSource();

                var consoleJourneyState = new ConsoleJourneyState
                {
                    NextMove = MainMenuJourney
                };
                while (!cancelTokenSource.IsCancellationRequested)
                {
                    if (consoleJourneyState.NextMove is null)
                    {
                        break;
                    }
                    consoleJourneyState = await consoleJourneyState.NextMove.Invoke(cancelTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                Console.Clear();
                Console.WriteLine($"{NewConsoleLines()}Operation has been cancelled...{NewConsoleLines()}");
                return;
            }
            catch (ConsoleException ex)
            {
                Console.WriteLine(ex.Message);
                await Task.Delay(6000);
            }
            catch (System.Exception)
            {
                Console.WriteLine($"{NewConsoleLines()}An unexpected exception occurred...{NewConsoleLines()}");
                return;
            }
        }
    }

    private Task<ConsoleJourneyState> MainMenuJourney(CancellationToken cancellationToken)
    {
        Console.Clear();
        Console.WriteLine($"{NewConsoleLines()}Welcome to the Npm.Renovator.ConsoleApp...{NewConsoleLines()}");
        Console.WriteLine($"This app can be used to renovate your Node.JS projects.{NewConsoleLines()}");
        
        var consoleChoice = GetChoice([
            $"1. View potential package upgrades {NewConsoleLines()}",
            $"2. Attempt to renovate project within your local file system. {NewConsoleLines()}"
        ]).ToString();

        Console.WriteLine($"{NewConsoleLines()}Please enter the local file system path to your package json (relative/full): {NewConsoleLines()}");

        var localFilePath = Console.ReadLine();

        if (string.IsNullOrEmpty(localFilePath))
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
        Console.WriteLine($"{NewConsoleLines()}Getting view. Please wait...{NewConsoleLines()}");

        var potentialUpgradesView = await _processingManager
            .GetCurrentPackageVersionAndPotentialUpgradesViewForLocalSystemRepoAsync(
                upgradeBuilder.LocalSystemFilePathToJson, token);

        if (!potentialUpgradesView.IsSuccess || potentialUpgradesView.Data is null)
        {
            throw new ConsoleException($"{NewConsoleLines()}Failed to retrieve potential upgrades view.{NewConsoleLines()}");
        }


        DisplayCurrentPackageVersionsAndPotentialUpgradesView(potentialUpgradesView.Data);


        Console.WriteLine($"{NewConsoleLines()}Do you want to return to the main menu (y/n)?{NewConsoleLines()}");

        var choice = GetYNChoice();

        if (choice == YNEnum.N)
        {
            throw new OperationCanceledException();
        }

        return new ConsoleJourneyState();
    }
    private async Task<ConsoleJourneyState> AttemptToRenovateRepoJourney(DependencyUpgradeBuilder upgradeBuilder, CancellationToken token)
    {
        Console.Clear();
        Console.WriteLine($"{NewConsoleLines()}Getting view. Please wait...{NewConsoleLines()}");

        var potentialUpgradesView = await _processingManager
            .GetCurrentPackageVersionAndPotentialUpgradesViewForLocalSystemRepoAsync(
                upgradeBuilder.LocalSystemFilePathToJson, token);

        if (!potentialUpgradesView.IsSuccess || potentialUpgradesView.Data is null)
        {
            throw new ConsoleException($"{NewConsoleLines()}Failed to retrieve potential upgrades view.{NewConsoleLines()}");
        }


        Dictionary<string, (string CurrentVersion, string NewVersion)> potentialUpgradeCandidates = potentialUpgradesView.Data.AllPackages
            .SelectMany(x => x.PotentialNewVersions
                .FastArraySelect(y => new KeyValuePair<string, (string CurrentVersion, string NewVersion)>(x.NameOnNpm, (x.CurrentVersion, y.CurrentVersion)))).ToDictionary();
        Console.Clear();
        if (potentialUpgradeCandidates.Count < 1)
        {
            throw new ConsoleException($"{NewConsoleLines()}No packages to upgrade...{NewConsoleLines()}");
        }
        else
        {
            Console.WriteLine($"{NewConsoleLines()}Please choose the upgrade(s) you want to try and renovate from this list of potential upgrades...{NewConsoleLines()}");


            bool exitRequested = false;
            while (!exitRequested)
            {
                if (upgradeBuilder.HasAnyUpgrades())
                {
                    Console.WriteLine($"{NewConsoleLines()}Current upgrades to try:{NewConsoleLines()}");
                    foreach(var upgradesToTry in upgradeBuilder.ReadonlyUpgradesView)
                    {
                        Console.WriteLine($"    {upgradesToTry.Key}: {upgradesToTry.Value}");
                    }
                    Console.WriteLine();
                }
                var upgradeListWithExit = potentialUpgradeCandidates.FastArraySelect(x => $"{x.Key}: {x.Value.CurrentVersion} -> {x.Value.NewVersion}{NewConsoleLines()}")
                    .Append(
                    $"Stop adding upgrades{NewConsoleLines()}"
                );

                var chosenOption = GetChoice(upgradeListWithExit);
                Console.Clear();
                if (chosenOption == upgradeListWithExit.Count())
                {
                    exitRequested = true;
                    continue;
                }
                else
                {
                    var chosenUpgrade = potentialUpgradeCandidates.ElementAt(chosenOption - 1);
                    upgradeBuilder.AddUpgrade(chosenUpgrade.Key, chosenUpgrade.Value.NewVersion);

                    potentialUpgradeCandidates.Remove(chosenUpgrade.Key);
                }
                if(upgradeListWithExit.Count() == 1)
                {
                    exitRequested = true;
                    continue;
                }
            }

            Console.WriteLine($"{NewConsoleLines()}Attempting to renovate repo. This may take a minute, please wait...{NewConsoleLines()}");

            var renovateResult = await _processingManager.AttemptToRenovateLocalSystemRepoAsync(upgradeBuilder, token);


            Console.Clear();
            if(!renovateResult.IsSuccess || !string.IsNullOrEmpty(renovateResult.ExceptionMessage) ||
                !string.IsNullOrEmpty(renovateResult.Data?.ExceptionOutput))
            {
                throw new ConsoleException($"{NewConsoleLines()}Failed to update repo with output: {NewConsoleLines(2)}    {renovateResult.Data?.ExceptionOutput ?? "None"}");
            }

            Console.WriteLine($"{NewConsoleLines()}Successfully renovated repo with output: {NewConsoleLines(2)}    {renovateResult.Data?.Output ?? "None"}{NewConsoleLines()}");
        }
        Console.WriteLine($"{NewConsoleLines()}Do you want to return to the main menu (y/n)?{NewConsoleLines()}");

        var choice = GetYNChoice();

        if (choice == YNEnum.N)
        {
            throw new OperationCanceledException();
        }

        return new ConsoleJourneyState();
    }
    private static int GetChoice(IEnumerable<string> options)
    {
        int selectedIndex = 0;
        int optionsCount = options.Count();
        ConsoleKey key;

        do
        {
            Console.WriteLine();

            for (int i = 0; i < optionsCount; i++)
            {
                if (i == selectedIndex)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("> " + options.ElementAt(i));
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine("  " + options.ElementAt(i));
                }
            }

            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            key = keyInfo.Key;

            if (key == ConsoleKey.UpArrow && selectedIndex > 0)
            {
                selectedIndex--;
            }
            else if (key == ConsoleKey.DownArrow && selectedIndex < optionsCount - 1)
            {
                selectedIndex++;
            }
            Console.Clear();
        } while (key != ConsoleKey.Enter);

        if(selectedIndex > optionsCount || selectedIndex < 0)
        {
            throw new ConsoleException("Please choose a valid option");
        }

        return selectedIndex + 1;
    }
    private static void DisplayCurrentPackageVersionsAndPotentialUpgradesView(
        CurrentPackageVersionsAndPotentialUpgradesView view)
    {
        Console.Clear();
        Console.WriteLine();
        foreach (var upg in view.AllPackages)
        {
            Console.WriteLine($"Package name: {upg.NameOnNpm}");
            Console.WriteLine($"Current version: {upg.CurrentVersion}");
            if (upg.PotentialNewVersions.Count < 1)
            {
                Console.WriteLine(NewConsoleLines());
                continue;
            }
            Console.WriteLine($"Potential upgrades: ");
            foreach (var newPackage in upg.PotentialNewVersions)
            {
                Console.WriteLine($"{NewConsoleLines()}    Version: {newPackage.CurrentVersion}");
                Console.WriteLine($"    Published: {newPackage.ReleaseDate}");
            }

            Console.WriteLine(NewConsoleLines());
        }
    }
    private static YNEnum GetYNChoice()
    {
        var choice = Console.ReadLine()?.ToLower();

        if(choice == YNEnum.N.GetDisplayName())
        {
            return YNEnum.N;
        }
        else if (choice == YNEnum.Y.GetDisplayName())
        {
            return YNEnum.Y;
        }
        else
        {
            throw new ConsoleException($"{NewConsoleLines()}Please enter y or n...{NewConsoleLines()}");
        }
    }
    private static string NewConsoleLines(int numberOf = 1)
    {
        if (numberOf == 1) return Environment.NewLine;
        var newLineBuilder = new StringBuilder();
        for (int i = 0; i < numberOf; i++)
        {
            newLineBuilder.Append(Environment.NewLine);
        }

        return newLineBuilder.ToString();
    }

}