using System.Diagnostics;
using System.Runtime.InteropServices;
using Npm.Renovator.Domain.Models.Views;
using Npm.Renovator.Domain.Services.Abstract;

namespace Npm.Renovator.Domain.Services.Concrete;

internal class NpmCommandService: INpmCommandService
{
    public async Task<NpmCommandResultsView> RunNpmInstall(string filePath)
    {
        var fullPath = Path.GetFullPath(filePath) ?? throw new InvalidOperationException("Unable to find json file");
        
        using var process = new Process(); 
        process.StartInfo = GetProcessStartInfo(fullPath);
        
        await process.StandardInput.WriteLineAsync("npm install");
        await process.StandardInput.WriteLineAsync("exit");

        var result = await Task.WhenAll(process.StandardOutput.ReadToEndAsync(),
            process.StandardError.ReadToEndAsync());

        await process.WaitForExitAsync();

        return new NpmCommandResultsView
        {
            Output = result.First(),
            Error = result.Last()
        };
    }

    
    private static ProcessStartInfo GetProcessStartInfo(string fullPath)
    {
        return new ProcessStartInfo
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = true,
            CreateNoWindow = true,
            WorkingDirectory = fullPath
        };
    }
}