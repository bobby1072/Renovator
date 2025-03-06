using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Npm.Renovator.Domain.Models;
using Npm.Renovator.Domain.Services.Abstract;

namespace Npm.Renovator.Domain.Services.Concrete;

internal class NpmCommandService: INpmCommandService
{
    private readonly ILogger<NpmCommandService> _logger;

    public NpmCommandService(ILogger<NpmCommandService> logger)
    {
        _logger = logger;
    }
    public async Task<NpmCommandResults> RunNpmInstallAsync(string localSystemFilePathToPackageJson, CancellationToken cancellationToken  = default)
    {
        var fullPath = Path.GetFullPath(localSystemFilePathToPackageJson) ?? throw new InvalidOperationException("Unable to find json file");
        
        using var process = new Process(); 
        process.StartInfo = GetProcessStartInfo(fullPath);
        
        await process.StandardInput.WriteLineAsync("npm install");
        await process.StandardInput.WriteLineAsync("exit");

        var result = await Task.WhenAll(process.StandardOutput.ReadToEndAsync(cancellationToken),
            process.StandardError.ReadToEndAsync(cancellationToken));

        await process.WaitForExitAsync(cancellationToken);

        var resultsView = new NpmCommandResults
        {
            Output = result.First(),
            Exception = result.Last()
        };

        if (!string.IsNullOrEmpty(resultsView.Exception))
        {
            _logger.LogError("Npm install failed: {Error}", resultsView.Exception);
        }
        
        return resultsView;
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