using Microsoft.Extensions.Logging;
using Npm.Renovator.Domain.Models;
using Npm.Renovator.Domain.Services.Abstract;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Npm.Renovator.Domain.Services.Concrete;

internal class NpmCommandService: INpmCommandService
{
    private readonly ILogger<NpmCommandService> _logger;

    public NpmCommandService(ILogger<NpmCommandService> logger)
    {
        _logger = logger;
    }
    public async Task<NpmCommandResults> RunNpmInstallAsync(string workingDirectory, CancellationToken cancellationToken  = default)
    {
        using var process = new Process(); 
        process.StartInfo = GetProcessStartInfo(workingDirectory);
        process.Start();

        await process.StandardInput.WriteLineAsync("npm install");
        
        await process.StandardInput.FlushAsync();
        process.StandardInput.Close();

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

    
    private static ProcessStartInfo GetProcessStartInfo(string workingDirectory)
    {
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        var shell = isWindows ? "cmd.exe" : "/bin/bash";
        return new ProcessStartInfo
        {
            FileName = shell,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory
        };
    }
}